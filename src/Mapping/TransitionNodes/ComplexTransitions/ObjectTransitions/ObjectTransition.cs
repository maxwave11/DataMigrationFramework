using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    public class ObjectTransition : ComplexTransition
    {
        /// <summary>
        /// The unique DataSet id of target system
        /// </summary>
        [XmlAttribute]
        public string TargetDataSetId { get; set; }

        /// <summary>
        /// Key definition element which describes how to get keys for source and for target objects respectively
        /// </summary>
        [XmlElement]
        public KeyTransition KeyTransition { get; set; }

        /// <summary>
        /// Indicates which objects will be transitted depend from their existence in target system. 
        /// <seealso cref="TransitMode"/>
        /// </summary>
        [XmlAttribute]
        public ObjectTransitMode TransitMode { get; set; }

        /// <summary>
        /// Migration expression to define context for current ObjectTransition
        /// </summary>
        [XmlAttribute]
        public string From { get; set; }

        /// <summary>
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        [XmlAttribute]
        public string RowsRange { get; set; }

        private Dictionary<int, int> _allowedRanges;

        public readonly List<TraceEntry> TraceEntries = new List<TraceEntry>();

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Magenta;
            ParseRowsRange();
            Validate();

            if (KeyTransition != null)
            {
                ChildTransitions.Insert(0, KeyTransition);
            }

            base.Initialize(parent);
        }

        protected virtual void Validate()
        {
            if (KeyTransition == null)
                throw new Exception($"{nameof(KeyTransition)} is required for {nameof(ObjectTransition)} element");
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            TraceEntries.Clear();

            if (ctx.Source == null)
                throw new InvalidOperationException($"Can't transit NULL Source. Use {nameof(ObjectSetTransition)} to link to some source and use {nameof(ObjectTransition)} within parent {nameof(ObjectSetTransition)}");

            ctx.ObjectTransition = this;
            var result = base.Transit(ctx);

            if (result.Continuation == TransitContinuation.SkipObject && ctx.Target?.IsNew == true)
            {
                //If object just created and skipped by migration logic - need to remove it from cache
                //becaus it is invalid object and we must disallow reference to this objects by any keys
                //If object is not new, it means that it already saved and passed/valid object
                var provider = Migrator.Current.Action.DefaultTargetProvider;
                var dataSet = provider.GetDataSet(TargetDataSetId);
                dataSet.RemoveObjectFromCache(ctx.Target.Key);
            }

            return result;
        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            ctx.SetCurrentValue(childNode.Name, ctx.Source);

            var result = base.TransitChild(childNode, ctx);
            if (childNode is KeyTransition)
                TraceLine("Key: " + ctx.Source.Key);
            return result;
        }

        protected override TransitContinuation GetContinuation(TransitResult result)
        {
            if (result.Continuation == TransitContinuation.SkipValue)
                return TransitContinuation.Continue;

            return base.GetContinuation(result);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            if (ctx?.Source != null)
                attributes += "RowNumber=" + ctx.Source["RowNumber"];

            base.TraceStart(ctx, attributes);
        }

        protected override void TraceEnd(ValueTransitContext ctx)
        {
            var tagName = this.GetType().Name;
            var traceMsg = $"</{tagName}>";
            TraceLine(traceMsg);
        }

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceEntry() { Mesage = msg, Color = color });
        }

        public override bool CanTransit(ValueTransitContext ctx)
        {
            if (ctx?.Source != null)
            {
                if (!IsRowIndexInRange((int)ctx.Source["RowNumber"]))
                    return false;
            }

            return base.CanTransit(ctx);
        }

        private bool IsRowIndexInRange(int rowIndex)
        {
            if (RowsRange.IsEmpty()) return true;

            return _allowedRanges.Any(i => i.Key <= rowIndex && rowIndex <= i.Value);
        }

        private void ParseRowsRange()
        {
            if (RowsRange.IsEmpty()) return;

            if (this._allowedRanges == null)
            {
                this._allowedRanges = new Dictionary<int, int>();

                foreach (string strRange in RowsRange.Split(','))
                {
                    if (strRange.Contains("-"))

                        this._allowedRanges.Add(Convert.ToInt32(strRange.Split('-')[0]), Convert.ToInt32(strRange.Split('-')[1]));
                    else
                        this._allowedRanges.Add(Convert.ToInt32(strRange), Convert.ToInt32(strRange));
                }
            }
        }
    }
}