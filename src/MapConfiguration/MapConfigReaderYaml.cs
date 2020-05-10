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

            var commandMapping = new Dictionary<string, Type>()
            {
                // { "transit-data", typeof(TransitDataCommand) },
                // { "transit", typeof(TransitValueCommand) },
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

            //TranslateMapConfig(yamlInput);
            var deserializer = builder.Build();

            var mapConfig = deserializer.Deserialize<MapConfig>(yamlInput);
            mapConfig.Initialize();

            return mapConfig;
        }
    }
   
}