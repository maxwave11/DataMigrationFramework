﻿using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class WriteTransitUnit : TransitUnit
    {
        public override TransitResult TransitValue(ValueTransitContext ctx)
        {
            TransitContinuation continuation = TransitContinuation.Continue;

            if (string.IsNullOrEmpty(ctx.TransitValue?.ToString()))
            {
                continuation = OnEmpty;
            }

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