using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReadTransitUnit : TransitUnit
    {
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var returnValue = Expression.IsJustString 
                ? ((IValuesObject)ctx.TransitValue).GetValue(Expression.Expression)
                : Expression.Evaluate(ctx);

            return new TransitResult(returnValue);
        }
    }   
}