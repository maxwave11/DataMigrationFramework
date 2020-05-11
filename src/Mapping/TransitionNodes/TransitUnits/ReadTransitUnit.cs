using System;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReadTransitUnit : TransitUnit
    {
        public string FromField { get; set; }
        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            var returnValue = FromField.IsNotEmpty() 
                ? ctx.Source.GetValue(FromField)
                : Expression.Evaluate(ctx);

            return new TransitResult(returnValue);
        }
        
        public static implicit operator ReadTransitUnit(string expression)
        {
            if (expression.IsEmpty())
                throw new InvalidOperationException("Expression can't be empty");
            
            if (MigrationExpression.IsExpression(expression))
                return new ReadTransitUnit() { Expression = expression };
            
            return new ReadTransitUnit() { FromField = expression };
        }

        public override string ToString()
        {
            if (FromField.IsNotEmpty())
                return $"{nameof(FromField)}: {FromField}";

            return $"{nameof(Expression)}: {Expression}";
        }
    }   
    
   
}