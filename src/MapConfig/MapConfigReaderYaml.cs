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
using YamlDotNet.Core;
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
                typeof(SqlDataSource),
                //transitions
                typeof(KeyTransition),
                typeof(TransitValueCommand),
                typeof(LookupValueTransitUnit),
                typeof(TransitUnit),
                typeof(Condition),
                typeof(TypeConvertTransitUnit),
                typeof(ReplaceTransitUnit),
                typeof(WriteMessageUnit),
                typeof(ObjectTransition),
                typeof(TransitDataCommand),
                typeof(GlobalObjectTransition),
            };

            var commandMapping = new Dictionary<string, Type>()
            {
                // { "transit-data", typeof(TransitDataCommand) },
                // { "transit", typeof(TransitValueCommand) },
                { "condition", typeof(Condition) },
                { "replace", typeof(ReplaceTransitUnit) },
                { "LOOKUP", typeof(LookupValueTransitUnit) },
                { "csv", typeof(CsvDataSource) },
                { "excel", typeof(ExcelDataSource) },
                { "csv-settings", typeof(CsvSourceSettings) },
                { "GET", typeof(TransitUnit) },
                { "SET", typeof(WriteTransitUnit) }
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