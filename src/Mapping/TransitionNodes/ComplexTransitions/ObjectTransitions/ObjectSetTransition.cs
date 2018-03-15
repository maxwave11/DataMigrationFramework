using System;
using System.CodeDom;
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
    public class ObjectSetTransition : ComplexTransition
    {
        #region XmlAttributes
        /// <summary>
        /// Call SaveObjects when transitioned objects count reached this value
        /// </summary>
        [XmlAttribute]
        public int SaveCount { get; set; }

        /// <summary>
        /// The query which can be parsed and recognized by Source object or Source Data provider (depend from <see cref="FetchMode"/>)
        /// </summary>
        [XmlAttribute]
        public string QueryToSource { get; set; }

        /// <summary>
        /// Determines how source objects will be fetched
        /// </summary>
        [XmlAttribute]
        public FetchMode FetchMode { get; set; }

        /// <summary>
        /// The name of provider from which should be fetched source objects
        /// </summary>
        [XmlAttribute]
        public string SourceProviderName { get; set; }

        /// <summary>
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        [XmlAttribute]
        public string RowsRange { get; set; }

        private Dictionary<int, int> _allowedRanges;
        #endregion

        #region Members

        private readonly List<IValuesObject> _transittedObjects = new List<IValuesObject>();

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        private IValuesObject _currentSourceObject;

        #endregion

        #region Methods

        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Magenta;
         
            if (string.IsNullOrEmpty(QueryToSource))
                throw new Exception($"{nameof(QueryToSource)} can't be empty in {nameof(ObjectSetTransition)}");

            ParseRowsRange();


            base.Initialize(parent);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes = $"{nameof(Name)}=\"{Name}\" {nameof(QueryToSource)}=\"{QueryToSource}\"";
            base.TraceStart(ctx, attributes);
        }

        protected virtual IEnumerable<IValuesObject> GetSourceObjects(ValueTransitContext ctx)
        {
            try
            {
                //need to fix this later and recognize expression more smart
                if (QueryToSource.StartsWith("{"))
                    QueryToSource = ExpressionEvaluator.EvaluateString(QueryToSource, ctx);

                if (FetchMode == FetchMode.SourceObject)
                    return ((IValueObjectsCollecion)ctx.Source).GetObjects(this.QueryToSource);
                
                
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

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var srcDataSet = GetSourceObjects(ctx);

            if (srcDataSet == null)
                 return new TransitResult(null);

            var rowNumber = 0;
            foreach (var sourceObject in srcDataSet)
            {
                rowNumber++;

                if (!IsRowIndexInRange(rowNumber))
                    continue;

                sourceObject.SetValue("RowNumber", rowNumber);

                _currentSourceObject = sourceObject;
                ctx.Target = null;
                
                var result = TransitChildren(ctx);

                if (result.Continuation == TransitContinuation.SkipObject)
                {
                    // TraceLine($"Object skipped (Key = {ctx.Source.Key})" + result.Message);
                    continue;
                }

                if (result.Continuation != TransitContinuation.Continue)
                {
                    TraceLine($"Breaking {nameof(ObjectSetTransition)}");
                    return result;
                }
            }

            SaveTransittedObjects();

            return new TransitResult(null);
        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            ctx.Source = _currentSourceObject;
            //reset cached source key because different nesetd transitions 
            //can use different source key evaluation logic
            ctx.Source.Key = String.Empty;

            var result =  base.TransitChild(childNode, ctx);

            if (result.Continuation == TransitContinuation.SkipObject || result.Continuation == TransitContinuation.SkipObjectSet)
                return result;

            var targetObjects = new List<IValuesObject>();

            var target = ctx.Target;
            if (target is IEnumerable<IValuesObject>)
            {
                targetObjects.AddRange((IEnumerable<IValuesObject>)target);
            }
            else
            {
                if (target != null)//target can be null if SkipObject activated
                    targetObjects.Add((IValuesObject)target);
            }

            MarkObjectsAsTransitted(targetObjects);
            //need to save after each child transition to avoid referencing to unsaved data
            TrySaveTransittedObjects();
         
            return result;
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

                _transittedObjects.Add(targetObject);
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
                var newObjectsCount = _transittedObjects.Count(i => i.IsNew);

                if (newObjectsCount > 0)
                    TraceLine($"New objects: {newObjectsCount}");

                var stopWath = new Stopwatch();
                stopWath.Start();

                Migrator.Current.Action.DefaultTargetProvider.SaveObjects(_transittedObjects);
                stopWath.Stop();

                TraceLine($"Saved {_transittedObjects.Count} objects, time: {stopWath.Elapsed.TotalMinutes} min");
            }
            catch (Exception ex)
            {
                var objectsInfo = _transittedObjects.Select(i => i.GetInfo()).Join("\n===========================\n");
                Tracer.TraceText("=====Error while saving transitted objects: " + ex + objectsInfo, this,ConsoleColor.Red);
                throw;
            }

            _transittedObjects.Clear();
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

        #endregion
    }
}