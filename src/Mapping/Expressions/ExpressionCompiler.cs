using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XQ.DataMigration.Data;

namespace XQ.DataMigration.Mapping.Expressions
{
    /// <summary>
    /// This util class allows to translate and compile "Migration expression" to delegate. 
    /// </summary>
    public class ExpressionCompiler
    {
        private readonly List<Type> _customTypes = new List<Type>();

        /// <summary>
        /// Compiles migration expression to delegate
        /// </summary>
        /// <param name="migrationExpression">Migration expression from mapping configuration</param>
        /// <param name="objTransitionType">The type of ObjectTransition from which expression should be executed</param>
        /// <returns>Compiled delegate</returns>
        internal ScriptRunner<object> Compile(string migrationExpression, Type objTransitionType) 
        {
            var translatedExpression = TranslateExpression(migrationExpression, objTransitionType);

            var importTypes = new List<Type>();
            importTypes.Add(typeof(IValuesObject));
            importTypes.AddRange(_customTypes.ToArray());

            if (objTransitionType != null)
                importTypes.Add(objTransitionType);

            var scriptOptions = ScriptOptions.Default
                .WithReferences(importTypes.Select(t => t.Assembly))
                .WithImports(importTypes.Select(t => t.Namespace).ToArray().Append("System").Append("System.Text"));
               
            var script = CSharpScript.Create<object>(translatedExpression, options: scriptOptions,  globalsType: typeof(ExpressionContext));
            
            ScriptRunner<object> runner = script.CreateDelegate();
           
            return runner;
        }

        /// <summary>
        ///Translates migration expression to C# compilable expression
        /// VERY preliminary description of Migration Expression
        /// Migration Expression has a custom syntax for easy and short reading in configuration file

        /// The common syntax is "{ C# code}". 
        /// Examples: "{ 5000 }", "{ 2*3 }", "{ var_a*var_b }", "{ var_a['index'] }" 
        /// See https://csharpeval.codeplex.com/ for detailed common syntax description
        /// Curly braces uses just for indicating that this string is Migration Expression and removes while translation 
        /// process
        /// Custom syntax translate rules:
        /// 1. Expression as fieldname: "some_filed" --> VALUE["some_filed"]
        /// 2. Fieldname without quatations: "{VALUE[some_filed]}" --> VALUE["some_filed"]
        /// 3. Composite expression: "{VALUE[some_filed]}/{VALUE[some_filed_2]}" --> Str(VALUE["some_filed"]) + "/" + Str(VALUE["some_filed_2"])
        /// Context varaibles in expression (see ExpressionContext for details):
        /// SRC - source object
        /// TARGET - target object
        /// VALUE - current migration value
        /// </summary>
        /// <param name="migrationExpression">Migration expression from mapping configuration</param>
        /// <param name="objTransitionType">The type of ObjectTransition from which expression should be executed</param>
        /// <returns>C# expression string</returns>
        private string TranslateExpression(string migrationExpression, Type objTransitionType)
        {
            //check that count of open and close curly braces are equal
            if (migrationExpression.Count(c => c == '{') != migrationExpression.Count(c => c == '}'))
                throw new Exception($"Expression {migrationExpression} is not valid. Check open and close brackets");

            var expression = migrationExpression;

            //cast to concrete ObjectTransition type
            if (objTransitionType != null)
                expression = expression.Replace(nameof(ExpressionContext.THIS), $"(({objTransitionType.Name}){nameof(ExpressionContext.THIS)})");

            //quotes conversion: 
            //'some text' => "some text"
            //''some text'' => 'some text'
            expression = expression.Replace("''", "~~~");//used in sql statements
            expression = expression.Replace("'", "\"");
            expression = expression.Replace("~~~", "'");

            //replace  curly braces to "#open" and "#close" to avoid problems with parsing string formats
            //which can contains curly braces
            //expression = expression.Replace("\\{", "#open").Replace("\\}", "#close");

            //convertion  SOME_EXPRESSION[fieldname] => ((IValuesObject)SOME_EXPRESSION)["fieldname"]
            var valuesObjectPrefixes = new[]
            {
                nameof(ExpressionContext.SRC),
                nameof(ExpressionContext.TARGET),
                nameof(ExpressionContext.VALUE),
                nameof(ExpressionContext.CUSTOM),
                nameof(ExpressionContext.GLOBAL),
                $@"{nameof(ExpressionContext.HISTORY)}.*?\)"
            };
            //braces regex wich define expression like [.....] or with nested braces [..[...]...]
            //nested braces can be for some nested queries instead of field name
            //Example with simple field name: 1. [field_name] => ["field_name"]
            //Example with expression (query): 2. [$.parent[?(@.jll_propertyid)].jll_propertyid] => ["$.parent[?(@.jll_propertyid)].jll_propertyid"]
            var bracesRegex = @"(\??)\s*\[(\s*(([^\[\]]*)?(\[[^\[\]]{1,}\])?([^\[\]]*)?)*\s*)\]";
            var regexp = new Regex($@"({ string.Join("|", valuesObjectPrefixes) }){ bracesRegex }");
            expression = regexp.Replace(expression, $"(({nameof(IValuesObject)})$1)$2[\"$3\"]");

          
            //determine type of exression. Expression like '{...}' should return pure value (no interpolation to string)
            if (new Regex(@"^{[^}]*}$").IsMatch(expression))
                return expression.Trim('{', '}');

            
            //convert plain string to C# string template
            //also trim $ symbol which force to convert expression to interpolated string
            expression = "$\"" + expression.Trim('$') + "\"";
            return expression;
        }

        /// <summary>
        /// Registers custom type in order to use it by any migration expression in MapConfig file.
        /// </summary>
        public void RegisterType<T>()
        {
            if (_customTypes.Contains(typeof(T)))
                throw new Exception(typeof(T) + " already registered");

            _customTypes.Add(typeof(T));
        }
    }
}