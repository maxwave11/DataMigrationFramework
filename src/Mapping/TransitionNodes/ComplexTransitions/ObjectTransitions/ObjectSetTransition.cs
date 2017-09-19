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
        public TransitionNode ObjectTransition { get; set; }

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
        public string QueryToSource { get; set; }


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

            if (string.IsNullOrEmpty(QueryToSource))
                throw new Exception($"{nameof(QueryToSource)} can't be empty in {nameof(ObjectSetTransition)}");


            ObjectTransition.Initialize(this);

            base.Initialize(parent);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes = $"{nameof(Name)}=\"{Name}\" {nameof(QueryToSource)}=\"{QueryToSource}\"";
            base.TraceStart(ctx, attributes);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var srcDataSet = GetSourceObjects(ctx);

            if (srcDataSet == null)
                 return new TransitResult(null);


            foreach (var sourceObject in srcDataSet)
            {
                if (!CanTransit(sourceObject))
                    continue;

                ctx.Source = sourceObject;
                var result = ObjectTransition.TransitInternal(ctx);

                if (result.Continuation == TransitContinuation.SkipObject)
                {
                    //TraceLine($"Object skipped (Key = {ctx.Source.Key})" + result.Message);
                    continue;
                }

                if (result.Continuation != TransitContinuation.Continue)
                {
                    TraceLine($"Breaking {nameof(ObjectSetTransition)}");
                    return result;
                }

                var targetObjects = new List<IValuesObject>();

                var target = ctx.Target;
                if (target is IEnumerable<IValuesObject>)
                {
                    targetObjects.AddRange((IEnumerable<IValuesObject>)target);
                }
                else
                {
                    if (target != null)
                        targetObjects.Add((IValuesObject)target);
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

            return new TransitResult(null);
        }

        protected virtual IEnumerable<IValuesObject> GetSourceObjects(ValueTransitContext ctx)
        {
            try
            {
                if (ctx.Source is IValueObjectsCollecion)
                    return ((IValueObjectsCollecion) ctx.Source).GetObjects(this.QueryToSource);

                var sourceProvider = Migrator.Current.Action.DefaultSourceProvider;

                if (SourceProviderName.IsNotEmpty())
                    sourceProvider = Migrator.Current.Action.MapConfig.GetSourceProvider(SourceProviderName);

                return sourceProvider.GetDataSet(QueryToSource);
            }
            catch (Exception ex)
            {
                Tracer.TraceError("Error while trying to get source datat set." + ex, this, null);
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
                TraceLine($"Saving {_transittedObjects.Count} objects...");
                var newObjectsCount = _transittedObjects.Count(i => i.Value.IsNew);

                if (newObjectsCount > 0)
                    TraceLine($"New objects: {newObjectsCount}");

                var stopWath = new Stopwatch();
                stopWath.Start();

                Migrator.Current.Action.DefaultTargetProvider.SaveObjects(_transittedObjects.Values);
                stopWath.Stop();

                TraceLine($"Saved {_transittedObjects.Count} objects, time: {stopWath.Elapsed.TotalMinutes} min");

              
                if (ObjectTransition is ObjectTransition)
                {
                    //NOTICE: Need to put objects to cache only after success saving 
                    //to avoid putting to cache skipped and invalid objects.
                    var provider = Migrator.Current.Action.DefaultTargetProvider;

                    var dataSet = provider.GetDataSet(((ObjectTransition) ObjectTransition).TargetDataSetId);
                    foreach (var transittedObject in _transittedObjects)
                    {
                        dataSet.PutObjectToCache(transittedObject.Value, transittedObject.Key);
                    }
                }

            }
            catch (Exception ex)
            {
                var objectsInfo = _transittedObjects.Select(i => i.Value.GetInfo()).Join("\n===========================\n");
                Tracer.TraceText("=====Error while saving transitted objects: " + ex + objectsInfo, this,ConsoleColor.Red);
                throw;
            }

            _transittedObjects.Clear();
        }

        private bool CanTransit(IValuesObject srcObject)
        {
           // if (!IsRowIndexInRange(rowIndex)) return false;

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