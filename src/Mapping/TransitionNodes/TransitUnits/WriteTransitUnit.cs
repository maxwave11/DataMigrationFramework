using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transit unit which writes value from ValueTransitContext to target object
    /// If Expression is just a property name -> unit writes value to appropriate property of target object
    /// If Expression is Migration expression -> unit exeuctes this expression
    /// </summary>
    public class WriteTransitUnit : TransitUnit
    {
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            TransitContinuation continuation = TransitContinuation.Continue;

            if (continuation == TransitContinuation.Continue)
            {
                if (Expression.Contains("{"))
                {
                    ExpressionEvaluator.Evaluate(Expression, ctx);
                }
                else
                {
                    ctx.Target.SetValue(Expression, ctx.TransitValue);
                }
            }

            return new TransitResult(TransitContinuation.Continue, ctx.TransitValue);
        }
    }   
}