using DataMigration.Data.DataSources;
using DataMigration.Enums;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;
using DataMigration.Pipeline.Expressions;
using DataMigration.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace DataMigration
{
    public class MapConfig
    {
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        public List<DataPipeline> Pipeline { get; set; } = new List<DataPipeline>();
        
        public Dictionary<string, MigrationExpression> _examples { get; set; }
        
        public static MapConfig Current  {get; private set; }

        public char DefaultDecimalSeparator { get; set; } = '.';
        
        public string DefaultCsvDelimiter { get; set; } = ";";
        public string SourceBaseDir { get; set; }
        
        public TraceMode TraceMode { get; set; }
        public static IEnumerable<Type> CustomTypes { get; set; } = new List<Type>();

        internal void Initialize()
        {
            Current = this;
            Pipeline.ForEach(i => i.Initialize());
        }

        public static MapConfig ReadFromFile(string yamlFilePath, IEnumerable<Type> customCommands) 
        {
            var yaml = File.ReadAllText(yamlFilePath);
            return ReadFromString(yaml, customCommands);
        }

        public static MapConfig ReadFromStream(Stream yamlFileStream, IEnumerable<Type> customCommands)
        {
            using StreamReader reader = new StreamReader(yamlFileStream);
            var yaml = reader.ReadToEnd();
            return ReadFromString(yaml, customCommands);
        }

        public static MapConfig ReadFromString(string yamlString, IEnumerable<Type> customCommands)
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
                { CommandUtils.GetCommandYamlName(typeof(MessageCommand)), typeof(MessageCommand) },

                { "csv", typeof(CsvDataSource) },
                { "composite-source", typeof(CompositeDataSource) },
                { "excel", typeof(ExcelDataSource) },
                { "sql", typeof(SqlDataSource) },

            };

            var builder = new DeserializerBuilder();

            (customCommands ?? new List<Type>())
                .Select(type => new KeyValuePair<string, Type>(type.Name, type))
                .Union(commandMapping)
                .ToList()
                .ForEach(type => builder = builder.WithTagMapping("!" + type.Key, type.Value));

            var deserializer = builder.Build();

            var mapConfig = deserializer.Deserialize<MapConfig>(yamlString);
            mapConfig.Initialize();

            return mapConfig;
        }
    }
}