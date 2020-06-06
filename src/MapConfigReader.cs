using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Utils;
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
                { CommandUtils.GetCommandYamlName(typeof(ReplaceCommandSet)), typeof(ReplaceCommandSet) },
                { CommandUtils.GetCommandYamlName(typeof(SetFlowCommand)), typeof(SetFlowCommand) },
                { CommandUtils.GetCommandYamlName(typeof(LookupCommand)), typeof(LookupCommand) },
                { CommandUtils.GetCommandYamlName(typeof(TypeConvertCommand)), typeof(TypeConvertCommand) },
                { CommandUtils.GetCommandYamlName(typeof(SetCommand)), typeof(SetCommand) },
                { CommandUtils.GetCommandYamlName(typeof(ConcatCommand)), typeof(ConcatCommand) },
                { CommandUtils.GetCommandYamlName(typeof(GetValueCommand)), typeof(GetValueCommand) },
                { CommandUtils.GetCommandYamlName(typeof(GetNotEmptyValueCommand)), typeof(GetNotEmptyValueCommand) },
                { CommandUtils.GetCommandYamlName(typeof(IfCommand)), typeof(IfCommand) },
                { CommandUtils.GetCommandYamlName(typeof(TraceCommand)), typeof(TraceCommand) },
                { CommandUtils.GetCommandYamlName(typeof(CommandSet<CommandBase>)), typeof(CommandSet<CommandBase>) },
                { CommandUtils.GetCommandYamlName(typeof(GetTargetCommand)), typeof(GetTargetCommand) },

                { "csv", typeof(CsvDataSource) },
                { "composite-source", typeof(CompositeDataSource) },
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