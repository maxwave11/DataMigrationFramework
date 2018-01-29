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


      

        public readonly List<TraceEntry> TraceEntries = new List<TraceEntry>();

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Magenta;
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

        protected override TransitResult EndTransitChild(TransitResult result, ValueTransitContext ctx)
        {
            if (result.Continuation == TransitContinuation.SkipValue)
                return new TransitResult(result.Value);

            return base.EndTransitChild(result, ctx);
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
    }
}