using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Expressions
{
    public class MigrationExpression: MigrationExpression<object>
    {
        public static implicit operator MigrationExpression(string expression)
        {
             return new MigrationExpression(expression);
        }

        public MigrationExpression(string expression) : base(expression)
        {
        }
    }

    public class MigrationExpression<T>
    {
        public string Expression { get; }
        private string _translatedExpression;
        private readonly ScriptRunner<T> _scriptRunner;
        public MigrationExpression(string expression)
        {
            Expression = expression;
            _scriptRunner = Compile(Expression, new List<Type>());
        }

        public T Evaluate(ValueTransitContext ctx)
        {
            try
            {
                var exprContext = new ExpressionContext(ctx);
                var task = _scriptRunner(exprContext);
                if (!task.IsCompleted)
                    throw new Exception("TASK NOT COMPLETED!!! ALARM!");
                
                return  task.Result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Compiles migration expression to delegate
        /// </summary>
        /// <param name="migrationExpression">Migration expression from mapping configuration</param>
        /// <param name="objTransitionType">The type of ObjectTransition from which expression should be executed</param>
        /// <returns>Compiled delegate</returns>
        private ScriptRunner<T> Compile(string migrationExpression, List<Type> customTypes)
        {
            _translatedExpression = ExpressionCompiler.TranslateExpression(migrationExpression);

            var importTypes = new List<Type>();
            importTypes.Add(typeof(IValuesObject));
            importTypes.AddRange(customTypes.ToArray());
            
            var scriptOptions = ScriptOptions.Default
                .WithReferences(importTypes.Select(t => t.Assembly))
                .WithImports(importTypes.Select(t => t.Namespace).ToArray().Append("System").Append("System.Text"))
                .WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release);

            var script = CSharpScript.Create<T>(_translatedExpression, options: scriptOptions, globalsType: typeof(ExpressionContext));

            try
            {
                ScriptRunner<T> runner = script.CreateDelegate();
                return runner;
                return null;
            }
            catch
            {
                throw;
            }
        }

        public override string ToString()
        {
            return  Expression;
        }
        
        public static implicit operator MigrationExpression<T>(string expression)
        {
            return new MigrationExpression<T>(expression);
        }
    }
}