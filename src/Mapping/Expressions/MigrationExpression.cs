using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Expressions
{
    public class MigrationExpression
    {
        public string Expression { get; }
        private string _translatedExpression;
        private readonly ScriptRunner<object> _scriptRunner;
        public bool IsJustString { get; } = true;
        public MigrationExpression(string expression)
        {
            Expression = expression;

            if (Expression.StartsWith("$") || Expression.StartsWith("=>"))
            {
                _scriptRunner = Compile(Expression, new List<Type>());
                IsJustString = false;
            }
        }

        public object Evaluate(ValueTransitContext ctx)
        {
            if (_scriptRunner == null)
                return Expression;
            
            try
            {
                var exprContext = new ExpressionContext(ctx);
                var task = _scriptRunner(exprContext);
                if (!task.IsCompleted)
                    throw new Exception("TASK NOT COMPLETED!!! ALARM!");
                
                return  task.Result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public string EvaluateString(ValueTransitContext ctx)
        {
            return Expression.IsEmpty() ? Expression : Evaluate(ctx)?.ToString();
        }

        /// <summary>
        /// Compiles migration expression to delegate
        /// </summary>
        /// <param name="migrationExpression">Migration expression from mapping configuration</param>
        /// <param name="objTransitionType">The type of ObjectTransition from which expression should be executed</param>
        /// <returns>Compiled delegate</returns>
        private ScriptRunner<object> Compile(string migrationExpression, List<Type> customTypes)
        {
            _translatedExpression = ExpressionCompiler.TranslateExpression(migrationExpression);

            var importTypes = new List<Type>();
            importTypes.Add(typeof(IValuesObject));
            importTypes.AddRange(customTypes.ToArray());


            var scriptOptions = ScriptOptions.Default
                .WithReferences(importTypes.Select(t => t.Assembly))
                .WithImports(importTypes.Select(t => t.Namespace).ToArray().Append("System").Append("System.Text"));

            var script = CSharpScript.Create<object>(_translatedExpression, options: scriptOptions, globalsType: typeof(ExpressionContext));

            try
            {
                ScriptRunner<object> runner = script.CreateDelegate();
                return runner;

            }
            catch
            {
                throw;
            }
        }

        public override string ToString()
        {
            return  $"{Expression}  ~~  {_translatedExpression}";
        }

        public static implicit operator MigrationExpression(string expression)
        {
            return new MigrationExpression(expression);
        }
    }
}