using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using ExpressionEvaluator;
using XQ.DataMigration.Data;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class ObjectTransition : TransitionNode
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
        public TransitMode TransitMode { get; set; }
        /// <summary>
        /// The name of provider from which should be fetched source objects
        /// </summary>
        [XmlAttribute]
        public string SourceProviderName { get; set; }
        #endregion

        #region Members

        public List<ValueTransitionBase> ValueTransitions { get; set; }

        private readonly Dictionary<string, IValuesObject> _transittedObjects = new Dictionary<string, IValuesObject>();

        private Dictionary<int, int> _allowedRanges;

        #endregion

        #region Methods

        public override void Initialize(TransitionNode parent)
        {
            ConsoleColor = ConsoleColor.White;
            Validate();
            KeyDefinition?.Initialize(this);
            ParseRowsRange();

            base.Initialize(parent);
        }

        public virtual void TransitAllObjects()
        {
            var objectIndex = 1;
            var savedCount = 0;

            Trace($">>>Transitting all objects from  source DataSet '{Name}'  to target DataSet'{TargetDataSetId}'");

            var srcDataSet = GetSourceDataSet();

            if (srcDataSet == null)
                return;

            foreach (var sourceObject in srcDataSet)
            {
                objectIndex++;

                if (!CanTransit(sourceObject, objectIndex)) continue;

                Trace($"Transition object ({Name}) №{++savedCount} SourceIndex: {objectIndex}");
                var targetObjects = TransitObject(sourceObject);
                if (targetObjects == null)
                {
                    Trace("Skipped", ConsoleColor.Yellow);
                    continue;
                }

                MarkObjectsAsTransitted(targetObjects);

                Trace("Object transitted", ConsoleColor.Green);

                TrySaveTransittedObject();
            }

            SaveTransittedObjects();
            Trace($"<<< Objects from source DataSet '{Name}' are transitted to target DataSet '{TargetDataSetId}'\n", ConsoleColor.DarkYellow);
            srcDataSet.Dispose();
        }

        public virtual ICollection<IValuesObject> TransitObject(IValuesObject source)
        {
            var objectKey = GetKeyFromSource(source);
            Trace($"ObjectKey[{objectKey}]");

            //don't transit objects with empty key
            if (objectKey.IsEmpty())
            {
                Trace("Source object key is empty. Skipping object.");
                return null;
            }
            var target = GetTargetObject(objectKey);
            if (target == null)
                return null;

            foreach (var valueTransition in ValueTransitions)
            {
                var ctx = new ValueTransitContext(source, target, source, this);
                var result = valueTransition.TransitValueInternal(ctx);

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

            return new[] { target };
        }

        public override List<TransitionNode> GetChildren()
        {
            return ValueTransitions.Cast<TransitionNode>().ToList();
        }

        protected virtual IValuesObject GetTargetObject(string key)
        {
            var provider = Migrator.Current.Action.TargetProvider;

            var existedObject = provider.GetDataSet(TargetDataSetId).GetObjectByKey(key, GetKeyFromTarget);

            if (TransitMode == TransitMode.OnlyExistedObjects)
                return existedObject;

            if (TransitMode == TransitMode.OnlyNewObjects && existedObject != null)
                return null;

            return existedObject ?? provider.CreateObject(TargetDataSetId);
        }

        protected virtual string GetKeyFromSource(IValuesObject sourceObject)
        {
            if (!sourceObject.Key.IsEmpty())
                return sourceObject.Key;

            var ctx = new ValueTransitContext(sourceObject, null, sourceObject, this);
            var transitResult = KeyDefinition.SourceKeyTransition.TransitValueInternal(ctx);
            if (transitResult.Continuation == TransitContinuation.Continue)
                sourceObject.Key = transitResult.Value?.ToString();
            if (transitResult.Continuation == TransitContinuation.RaiseError)
            {
                Trace($"Transition stopped on { Name }");
                throw new Exception("Can't transit source key ");
            }

            return sourceObject.Key;
        }

        protected virtual string GetKeyFromTarget(IValuesObject targetObject)
        {
            if (!targetObject.Key.IsEmpty())
                return targetObject.Key;

            var ctx = new ValueTransitContext(targetObject, null, targetObject, this);

            var transitResult = KeyDefinition.TargetKeyTransition.TransitValueInternal(ctx);
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

        private void TrySaveTransittedObject()
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
                Trace("Don't save objects due of MapAction.DoSave = false");
                return;
            }

            try
            {
              
                Trace("Saving....");

                var stopWath = new Stopwatch();
                stopWath.Start();

                Migrator.Current.Action.TargetProvider.SaveObjects(_transittedObjects.Values);
                stopWath.Stop();
                Trace($"Saved objects count: {_transittedObjects.Count()}, time: {stopWath.Elapsed.TotalMinutes} min");
            }
            catch (Exception ex)
            {
                Trace("=====Error while saving transitted objects: " + ex);
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

            Trace($"Finded object duplicate by key = {GetKeyFromTarget(targetObject)}");
            return true;
        }

        private void Trace(string traceMessage)
        {
            Debug.WriteLine(GetIndent() + traceMessage);
            Migrator.Current.InvokeTrace(GetIndent() + traceMessage, ConsoleColor);
        }

        private void Trace(string traceMessage, ConsoleColor color)
        {
            Debug.WriteLine(GetIndent() + traceMessage);
            Migrator.Current.InvokeTrace(GetIndent() + traceMessage, color);
        }


        #endregion
    }
}