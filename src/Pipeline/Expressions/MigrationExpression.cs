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
        private static Script _script;
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

            if (_script == null)
            {
                var importTypes = new List<Type>();
                importTypes.Add(typeof(IDataObject));
                importTypes.AddRange(customTypes.ToArray());
                
                var scriptOptions = ScriptOptions.Default
                    .WithReferences(importTypes.Union(MapConfig.CustomTypes).Select(t => t.Assembly))
                    .WithImports(importTypes.Union(MapConfig.CustomTypes).Select(t => t.Namespace).ToArray()
                        .Append("System")
                        .Append("System.Text")
                        .Append("System.Linq")
                        .Append("System.Collections.Generic")
                        .Append("System.Globalization")
                        .Append("System.Text.RegularExpressions"))
                    .WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release);

                _script = CSharpScript.Create("", options: scriptOptions, globalsType: typeof(ExpressionContext));
            }

            try
            {
                var script = _script.ContinueWith<T>(_translatedExpression);
                return script.CreateDelegate();
            }
            catch (Exception e)
            {
                throw;//left it for debuggin purposes
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