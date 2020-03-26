using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Expressions
{
    public class ExpressionEvaluator
    {
        private readonly Dictionary<string, Delegate> _compiledExpressionsCache = new Dictionary<string, Delegate>();

        public string EvaluateString(string expression, ValueTransitContext ctx)
        {
            if (expression.IsEmpty())
                return expression;

            return Evaluate(expression, ctx)?.ToString();
        }

        public object Evaluate(string expression, ValueTransitContext ctx)
        {
            //don't evaluate passed plain strings
            if (!expression.Contains("{"))
                return expression;


            var compiledFunction = GetCompiledFunction(expression, ctx.ObjectTransition?.GetType());

            try
            {
                var exprContext = new ExpressionContext(ctx);
                var task = compiledFunction(exprContext);
                if (!task.IsCompleted)
                    throw new Exception("TASK NOT COMPLETED!!! ALARM!");
                
                var value = task.Result;
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private ScriptRunner<object> GetCompiledFunction(string migrationExpression, Type objTransitionType)
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

            return (ScriptRunner<object>)result;
        }
    }
}