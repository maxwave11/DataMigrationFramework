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
    /// Transition unit which allows to get a value from some reference data set. For example, find asset and get his name by asset id or find
    /// city id by city name or by any other condition. Lookup condition determined by LookupExpr migration expression.
    /// </summary>
    internal class LookupValueTransitUnit: TransitUnit
    {
        [XmlAttribute]
        //DataSet id in which current transition will be search particular entry by expression determined in LookupExpr
        public string LookupDataSetId { get; set; }

        [XmlAttribute]
        //Migration expression which determine how to find particular entry
        //For each entry from Lookup DataSet the result of expression evaluation compared with current transition value 
        public string LookupExpr { get; set; }

        [XmlAttribute]
        //указывает, к какому провайдеру будет обращаться логика Lookup для поиска нужного значения
        //По умолчанию - Target
        public string ProviderName { get; set; }

        /// <summary>
        /// Specify what to do if lookup value not found
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

            if (LookupExpr.IsEmpty())
                throw new Exception($"{nameof(LookupExpr)} is required for { nameof(LookupValueTransitUnit)} element");

            
            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var key = base.Transit(ctx).Value?.ToString();
            IValuesObject lookupObject = null;
            string message = "";
            TransitContinuation continuation = TransitContinuation.Continue;
            _currentObjectTransition = ctx.ObjectTransition;
            if (key.IsNotEmpty())
            {
                lookupObject = GetLookupObjectByKey(key);

                if (lookupObject == null)
                {
                    TraceLine($"Warning: lookup object not found by key '{key}'");
                    continuation = this.OnNotFound;
                    if (continuation != TransitContinuation.Continue)
                        message = "Value transition interuppted because of empty value of transition " + this.Name;
                }
            }

            return new TransitResult(continuation, lookupObject, message);
        }

        private IValuesObject GetLookupObjectByKey(string key)
        {
            IDataProvider provider;
            if (ProviderName.IsEmpty())
                provider = Migrator.Current.Action.TargetProvider;
            else
            {
                provider = Migrator.Current.Action.MapConfig.GetDataProvider(ProviderName);
            }

            var dataSet = provider.GetDataSet(LookupDataSetId);

            return dataSet.GetObjectByKey(key, GetLookupExpressionValue);
        }

        private string GetLookupExpressionValue(IValuesObject lookupObject)
        {
            var ctx = new ValueTransitContext(lookupObject, lookupObject, lookupObject, _currentObjectTransition);
            return ExpressionEvaluator.EvaluateString(LookupExpr, ctx);
        }

        public override string ToString()
        {
            return base.ToString()+ 
                $"\n    LookupDataSetId: {LookupDataSetId}"+
                $"\n    LookupExpr: {LookupExpr}"+ 
                (ProviderName.IsNotEmpty() ?  $"\n    ProviderName: {ProviderName}" :"");
        }
    }
}