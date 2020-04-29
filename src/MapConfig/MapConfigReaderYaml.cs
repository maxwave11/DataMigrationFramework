using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace XQ.DataMigration.MapConfig
{

    class nnn : INodeTypeResolver
    {
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            throw new NotImplementedException();
        }
    }
    public class MapConfigReaderYaml
    {
        private readonly Stream _fileStream;
        private readonly String _fileName;
        //private readonly List<Type> _customElements = new List<Type>();

        public MapConfigReaderYaml(string fileName)
        {
          //  _fileStream = new FileStream(fileName, FileMode.Open);
            _fileName = fileName;
            //register default commonly used providers of source data
            //RegisterDataProvider(typeof(CsvDataSource));
            //RegisterDataProvider(typeof(ExcelDataSource));
            //RegisterDataProvider(typeof(SqlDataSource));
        }

        public MapConfigReaderYaml(Stream fileStream)
        {
            _fileStream = fileStream;
        }

        public MapConfig Read(IEnumerable<Type> customTypes)
        {
            var yamlInput = File.ReadAllText(_fileName);


            var types = new[] {
                //providers
                typeof(CsvDataSource),
                typeof(ExcelDataSource),
                typeof(SqlDataSource),
                //transitions
                typeof(KeyTransition),
                typeof(TransitValueCommand),
                typeof(LookupValueTransition),
                typeof(TransitUnit),
                typeof(Condition),
                typeof(TypeConvertTransitUnit),
                typeof(ReplaceTransitUnit),
                typeof(WriteMessageUnit),
                typeof(ObjectTransition),
                typeof(TransitDataCommand),
                typeof(GlobalObjectTransition),
            };


            TranslateMapConfig(yamlInput);

            //types
            //    .Union(customTypes)
            //    .ToList()
            //    .ForEach(type => builder = builder.WithTagMapping("!" + type.Name, type));
            var builder = new DeserializerBuilder();

            builder = builder
                .WithTagMapping("!transit-data", typeof(TransitDataCommand))
                .WithTagMapping("!transit", typeof(TransitValueCommand))
                .WithTagMapping("!condition", typeof(Condition))
                .WithTagMapping("!lookup", typeof(LookupValueTransition))
                .WithTagMapping("!replace", typeof(ReplaceTransitUnit))
                .WithTagMapping("!csv", typeof(CsvDataSource));


            var deserializer = builder.Build();

            var mapConfig = deserializer.Deserialize<MapConfig>(yamlInput);
            mapConfig.Initialize();

            return mapConfig;
        }

        private void TranslateMapConfig(string yamlInput) 
        {
            var input = new StringReader(yamlInput);

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(input);

            var document = yaml.Documents[0];


            var nodes = document.AllNodes;

            foreach(var node in nodes.OfType<YamlScalarNode>())
            {
                if (node.Value.StartsWith("$"))
                    node.Value = TranslateExpression(node.Value);

            }

            using (TextWriter writer = File.CreateText("MapConfig_Translated.yaml"))
            {
                var stream = new YamlStream(document);
                stream.Save(writer, false);
            }
        }

        private string TranslateExpression(string migrationExpression)
        {
            //check that count of open and close curly braces are equal
            if (migrationExpression.Count(c => c == '{') != migrationExpression.Count(c => c == '}'))
                throw new Exception($"Expression {migrationExpression} is not valid. Check open and close brackets");

            var expression = migrationExpression;

            //translate simplified global variable accessor directive '@': @variable => GLOBAL[variable]
            expression = new Regex(@"@([^\W]*)").Replace(expression, "GLOBAL[$1]");

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


        private void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new Exception("Error while mapping configuration parsing");
        }
    }
}