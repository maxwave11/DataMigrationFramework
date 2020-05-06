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
        // protected override void TraceEnd(ValueTransitContext ctx)
        // {
        //     var tagName = this.GetType().Name;
        //     var returnValue = ctx.TransitValue?.ToString();
        //     var returnValueType = ctx.TransitValue?.GetType().Name;
        //     var traceMsg = $"{tagName} Value: ({returnValueType.Truncate(30)}){returnValue.Truncate(40)}, To: {ctx.Target}.{Expression} ";
        //     TraceLine(traceMsg);
        // }

        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            if (Expression.IsJustString)
                ctx.Target.SetValue(Expression.Expression, ctx.TransitValue);
            else
                Expression.Evaluate(ctx);
            
            return new TransitResult(ctx.TransitValue);
        }
        
        public static implicit operator WriteTransitUnit(string expression)
        {
            return new WriteTransitUnit() { Expression = expression };
        }
    }   
}