using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Pipeline.Commands;
using YamlDotNet.Serialization;

namespace XQ.DataMigration
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
                { "REPLACE", typeof(ReplaceCommand) },
                { "FLOW", typeof(SetFlowCommand) },
                { "LOOKUP", typeof(LookupCommand) },
                { "TYPE", typeof(TypeConvertCommand) },
                { "SET", typeof(SetCommand) },
                { "GET", typeof(GetCommand) },
                { "IF", typeof(IfCommand) },
                { "TRACE", typeof(TraceCommand) },
                { "TRANSIT", typeof(ComplexCommand<CommandBase>) },

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