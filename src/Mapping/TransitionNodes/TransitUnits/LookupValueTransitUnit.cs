using System;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition unit which allows to find a specific value from some reference data set. For example, find an asset and get its name 
    /// by asset's id or find a city id by city name or by any other condition. Lookup condition determined by <c>LookupKeyExpr</c> or 
    /// <c>LookupAlternativeExpr</c> migration expression.
    /// </summary>
    internal class LookupValueTransitUnit: TransitUnit
    {
        
        [XmlAttribute]
        /// <summary>
        /// DataSet id in which current transition will try to find a particular object by expression determined in 
        /// <c>LookupKeyExpr</c> or <c>LookupAlternativeExpr</c>
        /// </summary>
        public string LookupDataSetId { get; set; }

        [XmlAttribute]
        /// <summary>
        /// Migration expression which determines how to evaluate key for each lookup object from <c>LookupDataSetId</c>. 
        /// This key will allow to find a specific object from lookup's DataSet. In general you should to find objects by this way 
        /// because it's very fast. 
        /// WARNING: Use this attribute carefully because all fetched objects from DataSet (if they wasn't fetched early by some another
        /// transition) will be hosted in local cache by this key.
        /// NOTE: If you want to find an object by another way (not by its key) then you should to use <c>LookupAlternativeExpr</c> attribue
        /// </summary>
        public string LookupKeyExpr { get; set; }

        [XmlAttribute]
        /// <summary>
        /// By using this attribute lookup logic will try to find a specific object by alternative expression (not by <c>LookupKeyExpr</c>). 
        /// But anyway <c>LookupKeyExpr</c> is required for correct objects caching.
        /// WARNING: Searching an object by this way is pretty slow! Use this attribute only if you have't data to find object by its key
        /// </summary>
        public string LookupAlternativeExpr { get; set; }

        [XmlAttribute]
        /// <summary>
        /// Specifies which DataProvider lookup logic will use to search particular objects
        /// Default Target provder by default
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Specifies migration behavior in case when lookup object wasn't found
        /// </summary>
        [XmlAttribute]
        public TransitContinuation OnNotFound { get; set; } = TransitContinuation.RaiseError;

        private ObjectTransition _currentObjectTransition;

        public override void Initialize(TransitionNode parent)
        {
            if (Expression.IsEmpty())
                Expression = "{VALUE}";

            if (LookupDataSetId.IsEmpty())
                throw new Exception($"{nameof(LookupDataSetId)} is required for { nameof(LookupValueTransitUnit)} element");

            if (LookupKeyExpr.IsEmpty())
                throw new Exception($"{nameof(LookupKeyExpr)} is required for { nameof(LookupValueTransitUnit)} element");

            
            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            IValuesObject lookupObject = null;
            string message = "";
            var continuation = TransitContinuation.Continue;

            var valueToFind = base.Transit(ctx).Value?.ToString();

            if (valueToFind.IsNotEmpty())
            {
                lookupObject = FindLookupObject(valueToFind, ctx);

                if (lookupObject == null)
                {
                    Migrator.Current.Tracer.TraceWarning($"Lookup object not found by key '{valueToFind}'\n", this);

                    continuation = this.OnNotFound;
                    if (continuation != TransitContinuation.Continue)
                        message = "Value transition interuppted because of empty value of transition " + this.Name;
                }
            }

            return new TransitResult(continuation, lookupObject, message);
        }

        private IValuesObject FindLookupObject(string valueToFind, ValueTransitContext ctx)
        {
            var mapConfig = Migrator.Current.MapConfig;

            var provider = ProviderName.IsEmpty()
                ? mapConfig.GetDefaultTargetProvider()
                : mapConfig.GetDataProvider(ProviderName);

            var queryToSource = LookupDataSetId.StartsWith("{")
                                        ? ExpressionEvaluator.EvaluateString(LookupDataSetId, ctx)
                                        : LookupDataSetId;

            var dataSet = provider.GetDataSet(queryToSource);

            var lookupObject = LookupAlternativeExpr.IsEmpty()
                ? dataSet.GetObjectByKey(valueToFind, EvaluateObjectKey)
                : dataSet.GetObjectByExpression(valueToFind, EvaluateAlternativeExpression, EvaluateObjectKey);

            return lookupObject;
        }

        private string EvaluateObjectKey(IValuesObject lookupObject)
        {
            var ctx = new ValueTransitContext(lookupObject, lookupObject, lookupObject, _currentObjectTransition);
            return ExpressionEvaluator.EvaluateString(LookupKeyExpr, ctx);
        }

        private string EvaluateAlternativeExpression(IValuesObject lookupObject)
        {
            var ctx = new ValueTransitContext(lookupObject, lookupObject, lookupObject, _currentObjectTransition);
            return ExpressionEvaluator.EvaluateString(LookupAlternativeExpr, ctx);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            var queryToSource = LookupDataSetId.StartsWith("{")
                                       ? ExpressionEvaluator.EvaluateString(LookupDataSetId, ctx)
                                       : LookupDataSetId;

            attributes += $" {nameof(LookupDataSetId)}=\"{ queryToSource }\"";
            base.TraceStart(ctx, attributes);
        }
    }
}