using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Call SaveObjects when transitioned objects count reached this value
        /// </summary>
        /// TODO move it to separate SaveTransition
        [XmlAttribute]
        public int SaveCount { get; set; }

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

        private readonly List<IValuesObject> _transittedObjects = new List<IValuesObject>();


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
                //becaus it's invalid and must be removed from cache to avoid any referencing to this object
                //by any migration logic (lookups, key ytansitions, etc.)
                //If object is not new, it means that it's already saved and passed by migration validation
                var provider = Migrator.Current.MapConfig.GetTargetProvider();
                provider.RemoveObjectFromCache(TargetDataSetId, ctx.Target.Key);
            }
            if (SaveCount > 0)
            {
                var target = ctx.Target;
             
                if (target != null)//target can be null if SkipObject activated
                {
                    if (!(target is IValuesObject))
                        throw new InvalidOperationException($"Target object should be of type {nameof(IValuesObject)}");

                    MarkObjectsAsTransitted(target);
                }

                //need to save after each child transition to avoid referencing to unsaved data
                if (_transittedObjects.Count >= SaveCount)
                    SaveTargetObjects(_transittedObjects);
            }


            return result;
        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            //Reset TransitValue by Source object before any children begins inside ObjectTrastition
            //Notice: if you want to pass TransitValue between transitions you have to place your
            //'connected' transition nodes inside ValueTransition
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

        //TODO: move it to separate save transition!
        #region TEMP    
        protected virtual void SaveTargetObjects(List<IValuesObject> targetObjects)
        {
            try
            {
                TraceLine($"Saving {targetObjects.Count} objects...");
                var newObjectsCount = targetObjects.Count(i => i.IsNew);

                if (newObjectsCount > 0)
                    TraceLine($"New objects: {newObjectsCount}");

                var stopWath = new Stopwatch();
                stopWath.Start();

                Migrator.Current.MapConfig.GetTargetProvider().SaveObjects(targetObjects);
                stopWath.Stop();


                TraceLine($"Saved {targetObjects.Count} objects, time: {stopWath.Elapsed.TotalMinutes} min");
            }
            catch (Exception ex)
            {
                var objectsInfo = targetObjects.Select(i => i.GetInfo()).Join("\n===========================\n");
                Tracer.TraceLine("=====Error while saving transitted objects: " + ex + objectsInfo, this, ConsoleColor.Red);
                throw;
            }

            targetObjects.Clear();
        }

        private void MarkObjectsAsTransitted(IValuesObject targetObject)
        {
            if (targetObject.IsEmpty() || targetObject.Key.IsEmpty())
                return;

            if (_transittedObjects.Contains(targetObject))
                return;

            _transittedObjects.Add(targetObject);
        }
        #endregion
    }
}