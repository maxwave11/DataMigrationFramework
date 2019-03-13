using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Antlr.Runtime.Tree;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.MapConfig
{
    /// <summary>
    /// Element defines logic by expressions or transitions for evaluating unique migration keys for source and target objects
    /// by existed data of this objects. Matching source and target objects based on this migration key and it's very important to
    /// define key evaluation logic. Since source and target objects are of different types (becaus source and target systems are different), 
    /// almost always you have to define key evaluation logic for source object and for target object respectively. 
    /// Migration key evaluated for source and target objects must be equal!
    /// </summary>
    public class KeyTransition: TransitionNode
    {
        /// <summary>
        /// Defines an expression which must evaluate a unique migration key for source object of transition
        /// </summary>
        [XmlAttribute]
        public string SourceKey { get; set; }

        /// <summary>
        /// Defines an expression which must evaluate a unique migration key for target object of transition
        /// </summary>
        [XmlAttribute]
        public string TargetKey { get; set; }

        /// <summary>
        /// Defines a set of transitions which must get a unique migration key for source object of transition. Use this attribute 
        /// if key definition is complex and SourceKey attribute is not enough
        /// </summary>
        [XmlElement]
        public ValueTransition SourceKeyTransition { get; set; }

        /// <summary>
        /// Defines a set of transitions which must get a unique migration key for target object of transition. Use this attribute 
        /// if key definition is complex and SourceKey attribute is not enough
        /// </summary>
        [XmlElement]
        public ValueTransition TargetKeyTransition { get; set; }

        private ObjectTransition _objectTransition;

        public override void Initialize(TransitionNode parent)
        {
            if (SourceKey.IsEmpty() && SourceKeyTransition == null)
                throw new Exception($"{nameof(SourceKey)} or nested {nameof(SourceKeyTransition)}  is required for {nameof(KeyTransition)} element");

            if (TargetKey.IsEmpty() && TargetKeyTransition == null)
                throw new Exception($"{nameof(TargetKey)} or nested {nameof(TargetKeyTransition)}  is required for {nameof(KeyTransition)} element");

            if (!(parent is ObjectTransition))
                throw new InvalidDataException($"{nameof(KeyTransition)} element can be nested only in {nameof(ObjectTransition)} element");

            if (SourceKey.IsNotEmpty())
            {
                if (SourceKeyTransition != null)
                    throw new Exception($"Setting {nameof(SourceKey)} and nested {nameof(SourceKeyTransition)} are not allowed at the same time");

                SourceKeyTransition = new ValueTransition { From = SourceKey};
            }
            if (TargetKey.IsNotEmpty())
            {
                if (TargetKeyTransition != null)
                    throw new Exception($"Setting {nameof(TargetKey)} and nested {nameof(TargetKeyTransition)} are not allowed at the same time");

                TargetKeyTransition = new ValueTransition { From = TargetKey};
            }

            _objectTransition = (ObjectTransition)parent;

            base.Initialize(parent);
            SourceKeyTransition.Initialize(this);
            TargetKeyTransition.Initialize(this);

            //Don't trace Key Transitions by default. 
            //Need to set TraceLevel explicitly if you want to trace this node
            if (TraceLevel== TraceLevel.Auto)
                TraceLevel = TraceLevel.None;

            SourceKeyTransition.Name = "SourceKeyTransition";
            TargetKeyTransition.Name = "TargetKeyTransition";

            //mark key transition by special colors
            SetColorRecursive(SourceKeyTransition, ConsoleColor.DarkGreen);
            SetColorRecursive(TargetKeyTransition, ConsoleColor.Blue);
        }

        private void SetColorRecursive(TransitionNode node, ConsoleColor color)
        {
            node.Color = color;
            var children = (node as ComplexTransition)?.ChildTransitions;
            if (children?.Any()  != true)
                return;

            foreach (var childNode in children)
            {
                SetColorRecursive(childNode, color);
            }
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            //don't transit objects with empty key
            var objectKey = GetKeyFromSource(ctx.Source);

            if (objectKey.IsEmpty())
            {
                TraceLine("Source object key is empty. Skipping object.");
                return new TransitResult(TransitContinuation.SkipObject, null);
            }
            var target = GetTargetObject(objectKey);

            if (target == null)
            {
                if (_objectTransition.TransitMode == ObjectTransitMode.OnlyExistedObjects)
                    TraceLine($"Target object not found with { nameof(TraceLevel)} = { nameof(ObjectTransitMode.OnlyExistedObjects)}");

                return new TransitResult(TransitContinuation.SkipObject, null, "Target object is not received");
            }

            if (target.IsNew)
                TraceLine($"New object created");
            else
                TraceLine($"Got existed object");
                
            ctx.Target = target;
            return new TransitResult(target);
        }

        protected virtual string GetKeyFromSource(IValuesObject sourceObject)
        {
            if (!sourceObject.Key.IsEmpty())
                return sourceObject.Key;

            var ctx = new ValueTransitContext(sourceObject, null, sourceObject, _objectTransition);
            var transitResult = SourceKeyTransition.TransitCore(ctx);

            if (transitResult.Continuation == TransitContinuation.Continue)
                sourceObject.Key = transitResult.Value?.ToString();

            if (transitResult.Continuation == TransitContinuation.RaiseError || transitResult.Continuation == TransitContinuation.Stop)
            {
                TraceLine($"Transition stopped on { Name }");
                throw new Exception("Can't transit source key ");
            }

            return sourceObject.Key;
        }

        protected virtual string GetKeyFromTarget(IValuesObject targetObject)
        {
            if (!targetObject.Key.IsEmpty())
                return targetObject.Key;

            var ctx = new ValueTransitContext(targetObject, null, targetObject, _objectTransition);
            var transitResult = TargetKeyTransition.TransitCore(ctx);
            targetObject.Key = transitResult.Value?.ToString();

            return targetObject.Key;
        }

        protected virtual IValuesObject GetTargetObject(string key)
        {
            var provider = Migrator.Current.MapConfig.GetDefaultTargetProvider();

            var existedObject = provider.GetDataSet(_objectTransition.TargetDataSetId).GetObjectByKey(key, GetKeyFromTarget);

            if (_objectTransition.TransitMode == ObjectTransitMode.OnlyExistedObjects)
            {
                return existedObject;
            }

            if (_objectTransition.TransitMode == ObjectTransitMode.OnlyNewObjects && existedObject != null)
            {
                TraceLine($"Object already exist, skipping, because TransitMode = TransitMode.OnlyNewObjects");
                return null;
            }

            if (existedObject != null)
                return existedObject;

            var newObject = provider.CreateObject(_objectTransition.TargetDataSetId);
            newObject.Key = key;
            provider.GetDataSet(_objectTransition.TargetDataSetId).PutObjectToCache(newObject, key);
            return newObject;
        }
    }
}
