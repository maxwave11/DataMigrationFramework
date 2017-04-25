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
        public KeyDefinition KeyDefinition { get; set; }

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
            KeyDefinition?.Initialize(parent);
            base.Initialize(parent);
        }

        protected virtual void Validate()
        {
            if (KeyDefinition == null)
                throw new Exception($"{nameof(KeyDefinition)} is required for {nameof(ObjectTransition)} element");
        }

        public void TraceObjectTransitionStart(ObjectTransition objectTransition, string objectKey)
        {
            TraceLine($"(Start object transition ({objectTransition.Name}) [{ objectKey }]");
        }

        public void TraceObjectTransitionEnd(ObjectTransition objectTransition)
        {
            TraceLine("(End object transition)");
        }

        public virtual ICollection<IValuesObject> TransitObject(IValuesObject source)
        {
            TraceEntries.Clear();

            var objectKey = GetKeyFromSource(source);

            TraceObjectTransitionStart(this, objectKey);

            //don't transit objects with empty key
            if (objectKey.IsEmpty())
            {
                Tracer.TraceText("Source object key is empty. Skipping object.", this, ConsoleColor.Yellow);
                return null;
            }
            var target = GetTargetObject(objectKey);
            if (target == null)
                return null;

            foreach (var valueTransition in ChildTransitions)
            {
                if (ActualTrace == TraceMode.True)
                    TraceLine("");

                var ctx = new ValueTransitContext(source, target, source, this);
                var result = valueTransition.TransitInternal(ctx);

                if (result.Continuation == TransitContinuation.SkipValue)
                {
                    continue;
                }

                if (result.Continuation == TransitContinuation.SkipObject)
                {
                    return null;
                }

                if (result.Continuation == TransitContinuation.Stop)
                {
                    throw new Exception("Object transition stopped");
                }
            }

            TraceObjectTransitionEnd(this);
            return new[] { target };
        }

        protected virtual string GetKeyFromSource(IValuesObject sourceObject)
        {
            if (!sourceObject.Key.IsEmpty())
                return sourceObject.Key;

            var ctx = new ValueTransitContext(sourceObject, null, sourceObject, this);
            var transitResult = KeyDefinition.SourceKeyTransition.TransitInternal(ctx);

            if (transitResult.Continuation == TransitContinuation.Continue)
                sourceObject.Key = transitResult.Value?.ToString();

            if (transitResult.Continuation == TransitContinuation.RaiseError)
            {
                TraceLine($"Transition stopped on { Name }");
                throw new Exception("Can't transit source key ");
            }

            return sourceObject.Key;
        }

        protected virtual IValuesObject GetTargetObject(string key)
        {
            var provider = Migrator.Current.Action.TargetProvider;

            var existedObject = provider.GetDataSet(TargetDataSetId).GetObjectByKey(key, GetKeyFromTarget);

            if (TransitMode == ObjectTransitMode.OnlyExistedObjects)
                return existedObject;

            if (TransitMode == ObjectTransitMode.OnlyNewObjects && existedObject != null)
            {
                TraceLine($"Object already exist, skipping, because TransitMode = TransitMode.OnlyNewObjects");
                return null;
            }

            if (existedObject != null)
                return existedObject;

            var newObject = provider.CreateObject(TargetDataSetId);
            newObject.Key = key;

            provider.GetDataSet(TargetDataSetId).PutObjectToCache(newObject, key);

            return newObject;
        }

        protected virtual string GetKeyFromTarget(IValuesObject targetObject)
        {
            if (!targetObject.Key.IsEmpty())
                return targetObject.Key;

            var ctx = new ValueTransitContext(targetObject, null, targetObject, this);
            var transitResult = KeyDefinition.TargetKeyTransition.TransitInternal(ctx);
            targetObject.Key = transitResult.Value?.ToString();

            return targetObject.Key;
        }

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceEntry() { Mesage = msg, Color = color });
        }

    }
}