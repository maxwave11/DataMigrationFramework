using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{

    public class NestedObjectTransition: ObjectTransition
    {
        public override TransitResult Transit(ValueTransitContext transitContext)
        {
            var nestedSource =  new ValuesObject(transitContext.Source);
            var ctx = new ValueTransitContext(nestedSource, null, null, null);
            return base.Transit(ctx);
        }
    }

    public class ObjectTransition: ComplexTransition
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

        //protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        //{
        //    //var source = ctx.Source;
        //    //var objectKey = source!=null ? GetKeyFromSource(source):"SOURCE NULL";
        //    base.TraceStart(ctx, $"Key='{objectKey}'");
        //}

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            TraceEntries.Clear();

            if (ctx.Source == null)
                throw new InvalidOperationException($"Can't transit NULL Source. Use {nameof(ObjectSetTransition)} to link to some source and use {nameof(ObjectTransition)} within parent {nameof(ObjectSetTransition)}");

            //don't transit objects with empty key
            //  var result =  KeyTransition.Transit(ctx);
            // var objectKey = GetKeyFromSource(source);


            //var target = GetTargetObject(objectKey);


            //if (target == null)
            //    return new TransitResult(TransitContinuation.SkipObject, null);

            // var valueTransitContext = new ValueTransitContext(source, null, source, this);
            ctx.ObjectTransition = this;
            return base.Transit(ctx);
            //foreach (var valueTransition in ChildTransitions)
            //{
            //    if (ActualTrace == TraceMode.True)
            //        TraceLine("");

            //    var valueTransitContext = new ValueTransitContext(source, target, source, this);
            //    var result = valueTransition.TransitInternal(valueTransitContext);

            //    if (result.Continuation == TransitContinuation.SkipValue)
            //    {
            //        continue;
            //    }

            //    if (result.Continuation == TransitContinuation.SkipObject)
            //    {
            //        return new TransitResult(TransitContinuation.SkipObject, null);
            //    }

            //    if (result.Continuation == TransitContinuation.Stop)
            //    {
            //        throw new Exception("Object transition stopped");
            //    }
            //}

            ////TraceObjectTransitionEnd(this);

        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            ctx.SetCurrentValue(childNode.Name, ctx.Source);
            return base.TransitChild(childNode, ctx);
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