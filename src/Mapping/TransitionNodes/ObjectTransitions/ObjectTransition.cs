﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using ExpressionEvaluator;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class ObjectTransition : ComplexTransition
    {
        #region XmlAttributes
        /// <summary>
        /// Call SaveObjects when transitioned objects count reached this value
        /// </summary>
        [XmlAttribute]
        public int SaveCount { get; set; }

        /// <summary>
        /// The unique DataSet id of source system
        /// </summary>
        [XmlAttribute]
        public string SourceDataSetId { get; set; }

        /// <summary>
        /// The unique DataSet id of target system
        /// </summary>
        [XmlAttribute]
        public string TargetDataSetId { get; set; }

        /// <summary>
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        [XmlAttribute]
        public string RowsRange { get; set; }

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
        /// <summary>
        /// The name of provider from which should be fetched source objects
        /// </summary>
        [XmlAttribute]
        public string SourceProviderName { get; set; }
        #endregion

        #region Members

        private readonly Dictionary<string, IValuesObject> _transittedObjects = new Dictionary<string, IValuesObject>();

        private Dictionary<int, int> _allowedRanges;

        public readonly List<TraceEntry> TraceEntries = new List<TraceEntry>();

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        #endregion

        #region Methods

        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Magenta;
            Validate();
            KeyDefinition?.Initialize(this);
            ParseRowsRange();
            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var objectIndex = 1;

            Tracer.TraceObjectSetTransitionStart(this);
            var srcDataSet = GetSourceDataSet();

            if (srcDataSet == null)
                return new TransitResult(TransitContinuation.Continue, null);

            foreach (var sourceObject in srcDataSet)
            {
                objectIndex++;

                if (!CanTransit(sourceObject, objectIndex))
                    continue;

                var targetObjects = TransitObject(sourceObject);

                if (targetObjects == null)
                {
                    Tracer.TraceSkipObject("Skipped", this, sourceObject);
                    continue;
                }

                MarkObjectsAsTransitted(targetObjects);
                TrySaveTransittedObjects();
            }

            SaveTransittedObjects();
            Tracer.TraceObjectSetTransitionEnd(this);
            srcDataSet.Dispose();

            return new TransitResult(TransitContinuation.Continue, null);
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

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceEntry() { Mesage = msg, Color = color });
        }

        protected virtual string GetKeyFromSource(IValuesObject sourceObject)
        {
            if (!sourceObject.Key.IsEmpty())
                return sourceObject.Key;

            var ctx = new ValueTransitContext(sourceObject,null, sourceObject, this);
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

            return existedObject ?? provider.CreateObject(TargetDataSetId);
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

        protected virtual void Validate()
        {
            if (KeyDefinition == null)
                throw new Exception($"{nameof(KeyDefinition)} is required for {nameof(ObjectTransition)} element");
        }

        private IDataSet GetSourceDataSet()
        {
            if (SourceDataSetId.IsEmpty())
                return null;

            try
            {
                ISourceProvider sourceProvider;
                if (SourceProviderName.IsNotEmpty())
                {
                    sourceProvider = Migrator.Current.Action.MapConfig.GetSourceProvider(SourceProviderName);
                }
                else
                {
                    sourceProvider = Migrator.Current.Action.SrcProvider;
                }

                return sourceProvider.GetDataSet(SourceDataSetId);
            }
            catch (Exception ex)
            {
                Tracer.TraceText("Error while trying to get source datat set." + ex);
                return null;
            }
        }

        private void MarkObjectsAsTransitted(IEnumerable<IValuesObject> targetObjects)
        {
            foreach (IValuesObject t in targetObjects)
            {
                if (t.IsEmpty())
                    continue;

                if (IsDuplicate(t))
                    continue;

                var targetKey = GetKeyFromTarget(t);

                if (targetKey.IsEmpty())
                    continue;

                if (!t.IsNew && _transittedObjects.ContainsKey(targetKey))
                    continue;

                _transittedObjects.Add(targetKey, t);
                var provider = Migrator.Current.Action.TargetProvider;

                provider.GetDataSet(TargetDataSetId).PutObjectToCache(t, GetKeyFromTarget);
            }
        }

        private void TrySaveTransittedObjects()
        {
            if (SaveCount > 0 && _transittedObjects.Count >= SaveCount)
            {
                SaveTransittedObjects();
            }
        }

        private void SaveTransittedObjects()
        {
            if (!_transittedObjects.Any())
                return;

            if (!Migrator.Current.Action.DoSave)
            {
                TraceLine("Don't saving objects due of MapAction.DoSave = false");
                return;
            }

            try
            {
                Tracer.TraceText("Saving....", this, ConsoleColor.DarkYellow);

                var stopWath = new Stopwatch();
                stopWath.Start();

                Migrator.Current.Action.TargetProvider.SaveObjects(_transittedObjects.Values);
                stopWath.Stop();
                Tracer.TraceText($"Saved objects count: {_transittedObjects.Count()}, time: {stopWath.Elapsed.TotalMinutes} min",this, ConsoleColor.DarkYellow);
            }
            catch (Exception ex)
            {
                Tracer.TraceText("=====Error while saving transitted objects: " + ex, this, ConsoleColor.Red);
                throw;
            }

            _transittedObjects.Clear();
        }

        private bool CanTransit(IValuesObject srcObject, int rowIndex)
        {
            if (!IsRowIndexInRange(rowIndex)) return false;

            if (Migrator.Current.Action.Filter.IsNotEmpty())
            {
                var expression = new CompiledExpression<bool>(Migrator.Current.Action.Filter);
                var registry = new TypeRegistry();
                registry.RegisterType<IValuesObject>();

                registry.RegisterSymbol("Src", srcObject);
                expression.TypeRegistry = registry;

                return expression.Eval();
            }

            return true;
        }

        private bool IsRowIndexInRange(int rowIndex)
        {
            if (RowsRange.IsEmpty()) return true;

            return this._allowedRanges.Any(i => i.Key <= rowIndex && rowIndex <= i.Value);
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

        private bool IsDuplicate(IValuesObject targetObject)
        {
            if (!targetObject.IsNew)
                return false;

            var keyValue = GetKeyFromTarget(targetObject);

            if (keyValue.IsEmpty())
                return false;

            var findedObject = _transittedObjects.ContainsKey(keyValue) ? _transittedObjects[keyValue] : null;

            if (findedObject == null)
                return false;

            TraceLine($"Finded object duplicate by key = {GetKeyFromTarget(targetObject)}");
            return true;
        }

        #endregion
    }
}