using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressionEvaluator;
using XQ.DataMigration.Data;
using XQ.DataMigration.Utils;

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
        internal Delegate Compile(string migrationExpression, Type objTransitionType) 
        {
            var translatedExpression = TranslateExpression(migrationExpression, objTransitionType);
            var compiledExpr = new CompiledExpression(translatedExpression);

            var typeRegistry = new TypeRegistry();
            typeRegistry.RegisterDefaultTypes();
            typeRegistry.RegisterType(nameof(IValuesObject), typeof(IValuesObject));

            if(objTransitionType != null)
                typeRegistry.RegisterType(objTransitionType.Name, objTransitionType);

            _customTypes.ForEach(type => typeRegistry.RegisterType(type.Name, type));
            compiledExpr.TypeRegistry = typeRegistry;
            var result = compiledExpr.ScopeCompile<ExpressionContext>();
            return result;
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
            //check that count of open and close brackets are equal
            if (migrationExpression.Count(c => c == '{') != migrationExpression.Count(c => c == '}'))
                throw new Exception($"Expression {migrationExpression} is not valid. Check open and close brackets");

            //if passed field name, convert it to expression {VALUE[fieldname]}
            var newExpression = migrationExpression.Contains("{") ? migrationExpression : "{" + nameof(ExpressionContext.VALUE) + "[" + migrationExpression + "]}";

            //cast to concrete ObjectTransition type
            if(objTransitionType != null)
                newExpression = newExpression.Replace(nameof(ExpressionContext.THIS), $"(({objTransitionType.Name}){nameof(ExpressionContext.THIS)})");

            //convertion  'some text' => "some text"
            newExpression = newExpression.Replace('\'', '"');

            //replace  curly braces to "#open" and "#close" to avoid problems with parsing string formats
            //which can contains curly braces
            newExpression = newExpression.Replace("\\{", "#open").Replace("\\}", "#close");

            //convertion  SOME_EXPRESSION[fieldname] => ((IValuesObject)SOME_EXPRESSION)["fieldname"]
            var valuesObjectPrefixes = new[]
            {
                nameof(ExpressionContext.SRC),
                nameof(ExpressionContext.VALUE),
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
            var bracesRegex = @"\s*\[(\s*(([^\[\]]*)?(\[[^\[\]]{1,}\])?([^\[\]]*)?)*\s*)\]";
            var regexp = new Regex($@"({string.Join("|", valuesObjectPrefixes)}){bracesRegex}");
            newExpression = regexp.Replace(newExpression, $"(({nameof(IValuesObject)})$1)[\"$2\"]");

            //determine if expression is string template 
            regexp = new Regex(@"^\s*{([^}]*)}\s*$");
            string retVal;
            if (regexp.IsMatch(newExpression))
            {
                retVal = regexp.Replace(newExpression, "$1");
            }
            else
            {
                //build templated string
                regexp = new Regex(@"{(.*?[^\\])}");
                newExpression = regexp.Replace(newExpression, "#splitStr($1)#split");
                var reformatted = newExpression.Split(new [] { "#split"},StringSplitOptions.None)
                                               .Where(i => i.IsNotEmpty())
                                               .Select(str => str.StartsWith("Str(") ? str.Trim() : $"\"{ str }\"");
                retVal = String.Join(" + ", reformatted);
            }
            retVal = retVal.Replace("#open", "{").Replace("#close", "}");
            return retVal;
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