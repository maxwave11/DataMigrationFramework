using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace XQ.DataMigration.MapConfiguration
{
    public class MapConfigReaderYaml
    {
        private string _yaml;

        public MapConfigReaderYaml(string fileName)
        {
            _yaml = File.ReadAllText(fileName);
        }

        public MapConfigReaderYaml(Stream fileStream)
        {
            using (StreamReader reader = new StreamReader(fileStream))
            {
                _yaml = reader.ReadToEnd();
            }
        }

        public MapConfig Read(IEnumerable<Type> customTypes)
        {
            var commandMapping = new Dictionary<string, Type>()
            {
                { "condition", typeof(IfTransition) },
                { "REPLACE", typeof(ReplaceTransitUnit) },
                { "FLOW", typeof(FlowTransition) },
                { "LOOKUP", typeof(LookupValueTransitUnit) },
                { "TYPE", typeof(TypeConvertTransitUnit) },
                { "SET", typeof(WriteTransitUnit) },
                { "GET", typeof(ReadTransitUnit) },
                { "IF", typeof(IfTransition) },
                { "TRACE", typeof(TraceTransitUnit) },
                { "TRANSIT", typeof(ComplexTransition<TransitionNode>) },

                { "csv", typeof(CsvDataSource) },
                { "excel", typeof(ExcelDataSource) },
                { "csv-settings", typeof(CsvSourceSettings) },
            
            };

            var builder = new DeserializerBuilder();

            customTypes
                .Select(type => new KeyValuePair<string, Type>(type.Name, type))
                .Union(commandMapping)
                .ToList()
                .ForEach(type => builder = builder.WithTagMapping("!" + type.Key, type.Value));

            var deserializer = builder
                //.WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), s => s.InsteadOf<ObjectNodeDeserializer>())
                .Build();

            var mapConfig = deserializer.Deserialize<MapConfig>(_yaml);
            mapConfig.Initialize();

            return mapConfig;
        }
    }
    
    
    public class ValidatingNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer _nodeDeserializer;

        public ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer)
        {
            _nodeDeserializer = nodeDeserializer;
        }

        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (_nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value))
            {
                var context = new ValidationContext(value, null, null);
               
                //Console.WriteLine(value.GetType().Name + " " +  value);
                Validator.ValidateObject(value, context, true);
                return true;
            }
            return false;
        }
    }
   
}