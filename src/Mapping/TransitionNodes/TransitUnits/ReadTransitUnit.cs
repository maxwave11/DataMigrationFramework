using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    
    public class ReadKeyTransition : ComplexTransition<ReadTransitUnit>
    {
        protected override TransitResult TransitChild(ReadTransitUnit childTransition, ValueTransitContext ctx)
        {
            var result =  base.TransitChild(childTransition, ctx);
            return new TransitResult(result.Value?.ToString() + "/");
        }

        public static implicit operator ReadKeyTransition(string expression)
        {
            return new ReadKeyTransition() { expression };
        }
    }   
    public class ReadTransitUnit : TransitUnit
    {
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var returnValue = Expression.IsJustString 
                ? ((IValuesObject)ctx.Source).GetValue(Expression.Expression)
                : Expression.Evaluate(ctx);

            return new TransitResult(returnValue);
        }
        
        public static implicit operator ReadTransitUnit(string expression)
        {
            return new ReadTransitUnit() { Expression = expression };
        }
    }   
    
   
}