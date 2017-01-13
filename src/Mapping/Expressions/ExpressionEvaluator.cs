using System;
using System.Collections.Generic;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.Expressions
{
    internal class ExpressionEvaluator
    {
        private readonly Dictionary<string, Delegate> _compiledExpressionsCache = new Dictionary<string, Delegate>();

        public string EvaluateString(string migrationExpression, ValueTransitContext ctx)
        {
            return Evaluate(migrationExpression, ctx)?.ToString();
        }

        public object Evaluate(string expression, ValueTransitContext ctx)
        {
            var compiledFunction = GetCompiledFunction(expression, ctx.ObjectTransition.GetType());

            try
            {
                var exprContext = new ExpressionContext(ctx);
                var value = compiledFunction(exprContext);
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Func<ExpressionContext, object> GetCompiledFunction(string migrationExpression, Type objTransitionType)
        {
            Delegate result;
            if (!_compiledExpressionsCache.TryGetValue(migrationExpression, out result))
            {
                try
                {
                    result = Migrator.Current.ExpressionCompiler.Compile(migrationExpression, objTransitionType);
                    _compiledExpressionsCache.Add(migrationExpression, result);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return (Func<ExpressionContext, object>)result;
        }
    }
}