using System;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// TransitInternal unit which writes incoming value from ValueTransitContext to target object
    /// If Expression is just a property name -> unit writes value to appropriate property of target object
    /// If Expression is Migration expression -> unit exeuctes this expression
    /// </summary>
    public class WriteTransitUnit : TransitUnit
    {
        public string ToField { get; set; }

        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            if (ToField.IsNotEmpty())
                ctx.Target.SetValue(ToField, ctx.TransitValue);
            else
                Expression.Evaluate(ctx);
            
            return new TransitResult(ctx.TransitValue);
        }
        
        public static implicit operator WriteTransitUnit(string expression)
        {
            if (expression.IsEmpty())
                throw new InvalidOperationException("Expression can't be empty");
            
            if (expression.StartsWith("=>"))
                return new WriteTransitUnit() { Expression = expression.TrimStart('=','>') };
            
            return new WriteTransitUnit() { ToField = expression };
        }
    }   
}