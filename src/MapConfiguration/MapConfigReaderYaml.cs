using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

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
                { "CONCAT", typeof(ConcatReadTransition) },
                { "SET", typeof(WriteTransitUnit) },
                { "GET", typeof(ReadTransitUnit) },
                { "IF", typeof(IfTransition) },
                { "TRACE", typeof(TraceTransitUnit) },

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

            var deserializer = builder.Build();

            var mapConfig = deserializer.Deserialize<MapConfig>(_yaml);
            mapConfig.Initialize();

            return mapConfig;
        }
    }
   
}