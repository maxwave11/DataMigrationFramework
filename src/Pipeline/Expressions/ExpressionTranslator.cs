using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace XQ.DataMigration.Pipeline.Expressions
{
    /// <summary>
    /// This util class allows to translate and compile "Migration expression" to delegate. 
    /// </summary>
    public static class ExpressionCompiler
    {
        /// <summary>
        /// !DESCRIPTION OBSOLETE!
        /// Translates migration expression to C# compilable expression
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
        /// <returns>C# expression string</returns>
        public static  string TranslateExpression(string migrationExpression)
        {
            string expression = migrationExpression;

            bool isString = false;
            if (migrationExpression.StartsWith("$"))
            {
                expression = expression.TrimStart('$');
                isString = true;
            }

            //check that count of open and close curly braces are equal
            if (migrationExpression.Count(c => c == '{') != migrationExpression.Count(c => c == '}'))
                throw new Exception($"Expression {migrationExpression} is not valid. Check open and close brackets");

            //translate simplified global variable accessor directive '%': %variable% => Variables[variable]
            expression = new Regex(@"%([^%]*)%").Replace(expression, $"{nameof(ExpressionContext.Variables)}['$1']");

            //translate simplified values object accessor :
            //<field_name> => SRC[field_name]
            //<field_name:type> => GetValueFromSource<type>('field_name')
            var regex = new Regex(@"<([^<>]*?)(\:((int|double|float|long|decimal|bool|string|DateTime)\??))?>");
            var match = regex.Match(expression);
            var group = match.Groups[3];
            if (match.Success)
            {
                if (group.Success)
                {
                    expression = regex.Replace(expression, $"{nameof(ExpressionContext.GetValueFromSource)}<$3>('$1')");
                }
                else
                {
                    expression = regex.Replace(expression, $"{nameof(ExpressionContext.SRC)}[$1]");
                }
            }   
            
            //quotes conversion: 
            //'some text' => "some text"
            //''some text'' => 'some text'
            expression = expression.Replace("'", "\"");

            //replace  curly braces to "#open" and "#close" to avoid problems with parsing string formats
            //which can contains curly braces
            //expression = expression.Replace("\\{", "#open").Replace("\\}", "#close");

            //convertion  SOME_EXPRESSION[fieldname] => SOME_EXPRESSION["fieldname"]
            var valuesObjectPrefixes = new[]
            {
                nameof(ExpressionContext.SRC),
                nameof(ExpressionContext.TARGET),
                nameof(ExpressionContext.VALUE_OBJECT),
            };
            
            //braces regex wich define expression like [.....] or with nested braces [..[...]...]
            //nested braces can be for some nested queries instead of field name
            //Example with simple field name: 1. [field_name] => ["field_name"]
            //Example with expression (query): 2. [$.parent[?(@.jll_propertyid)].jll_propertyid] => ["$.parent[?(@.jll_propertyid)].jll_propertyid"]
            var bracesRegex = @"(\??)\s*\[(\s*(([^\[\]]*)?(\[[^\[\]]{1,}\])?([^\[\]]*)?)*\s*)\]";
            var regexp = new Regex($@"({ string.Join("|", valuesObjectPrefixes) }){ bracesRegex }");
            expression = regexp.Replace(expression, $"$1$2[\"$3\"]");

            return isString ? "$\"" + expression + "\"" : expression;
        }
    }
}