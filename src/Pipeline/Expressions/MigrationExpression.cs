using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using XQ.DataMigration.Data;

namespace XQ.DataMigration.Pipeline.Expressions
{
    public sealed class MigrationExpression: MigrationExpression<object>
    {
        public static implicit operator MigrationExpression(string expression)
        {
             return new MigrationExpression(expression);
        }

        public MigrationExpression(string expression) : base(expression) { }
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
            var exprContext = new ExpressionContext(ctx);
            var task = _scriptRunner(exprContext);
            if (!task.IsCompleted)
               throw new Exception("TASK NOT COMPLETED!!! ALARM!");
                
            return  task.Result;
        }

        /// <summary>
        /// Compiles migration expression to delegate
        /// </summary>
        /// <param name="migrationExpression">Migration expression from mapping configuration</param>
        /// <param name="customTypes">Custom types to include in compilation process</param>
        /// <returns>Compiled delegate</returns>
        private ScriptRunner<T> Compile(string migrationExpression, List<Type> customTypes)
        {
            _translatedExpression = ExpressionCompiler.TranslateExpression(migrationExpression);

            var importTypes = new List<Type>();
            importTypes.Add(typeof(IValuesObject));
            importTypes.AddRange(customTypes.ToArray());
            
            var scriptOptions = ScriptOptions.Default
                .WithReferences(importTypes.Select(t => t.Assembly))
                .WithImports(importTypes.Select(t => t.Namespace).ToArray().Append("System").Append("System.Text").Append("System.Linq"))
                .WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release);

            var script = CSharpScript.Create<T>(_translatedExpression, options: scriptOptions, globalsType: typeof(ExpressionContext));

            var runner = script.CreateDelegate();
            return runner;
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