using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Xml.Serialization;
using ExpressionEvaluator;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class ObjectSetTransition : TransitionNode
    {
    
        [XmlElement]
        public ObjectTransition ObjectTransition { get; set; }

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
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        [XmlAttribute]
        public string RowsRange { get; set; }

       
     
        /// <summary>
        /// The name of provider from which should be fetched source objects
        /// </summary>
        [XmlAttribute]
        public string SourceProviderName { get; set; }
        #endregion

        #region Members

        private readonly Dictionary<string, IValuesObject> _transittedObjects = new Dictionary<string, IValuesObject>();

        private Dictionary<int, int> _allowedRanges;


        private MigrationTracer Tracer => Migrator.Current.Tracer;

        #endregion

        #region Methods

        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Magenta;
            
            ParseRowsRange();

            if (ObjectTransition == null)
                throw new Exception($"{nameof(ObjectTransition)} can't be empty");

            ObjectTransition.Initialize(this);

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

                ctx = new ValueTransitContext(sourceObject,null, sourceObject, ObjectTransition);
                var result = ObjectTransition.TransitInternal(ctx);

                if (result.Continuation == TransitContinuation.SkipObject)
                    continue;
                if (result.Continuation == TransitContinuation.Stop)
                    return new TransitResult(TransitContinuation.Stop, null);

                var targetObjects = new List<IValuesObject>();

                if (result.Value is IEnumerable<IValuesObject>)
                {
                    targetObjects.AddRange((IEnumerable<IValuesObject>)result.Value);
                }
                else
                {
                    if (result.Value!=null)
                        targetObjects.Add((IValuesObject)result.Value);
                }

                if (!targetObjects.Any())
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

        protected IDataSet GetSourceDataSet()
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
            foreach (IValuesObject targetObject in targetObjects)
            {
                if (targetObject.IsEmpty())
                    continue;

                var targetKey = targetObject.Key;

                if (targetKey.IsEmpty())
                    continue;

                _transittedObjects[targetKey] = targetObject;
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

        #endregion
    }
}