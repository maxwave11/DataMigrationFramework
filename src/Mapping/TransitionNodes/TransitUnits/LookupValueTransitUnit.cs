using System;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition unit which allows to get a value from some reference data set. For example, find asset and get his name by asset id or find
    /// city id by city name or by any other condition. Lookup condition determined by LookupExpr migration expression.
    /// </summary>
    public class LookupValueTransitUnit: TransitUnit
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

        public LookupValueTransitUnit()
        {
            OnEmpty = TransitContinuation.RaiseError;
        }

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

        protected override object TransitValueInternal(ValueTransitContext ctx)
        {
            var stringValue = base.TransitValueInternal(ctx)?.ToString();
            if (stringValue.IsEmpty())
                return null;

            Trace(GetIndent(10)  +  "Lookup key = " + stringValue, ctx);
            var lookupObject = GetLookupObjectByKey(stringValue);
            return lookupObject;
        }

        public virtual IValuesObject GetLookupObjectByKey(string key)
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

        public virtual string GetLookupExpressionValue(IValuesObject lookupObject)
        {
            var ctx = new ValueTransitContext(lookupObject, lookupObject, lookupObject, ObjectTransition);
            return ExpressionEvaluator.EvaluateString(LookupExpr, ctx);
        }

        public override string GetInfo()
        {
            return base.GetInfo()+ 
                $"\n{GetIndent(5)}LookupDataSetId:{LookupDataSetId}"+
                $"\n{GetIndent(5)}LookupExpr:{LookupExpr}"+ 
                (ProviderName.IsNotEmpty() ?  $"\n{GetIndent(5)}ProviderName:{ProviderName}" :"");
        }
    }
}