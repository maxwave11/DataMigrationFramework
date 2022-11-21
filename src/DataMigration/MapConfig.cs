using DataMigration.Data.DataSources;
using DataMigration.Enums;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;
using DataMigration.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;

namespace DataMigration
{
    public class MapConfig
    {
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        public IList<IDataPipeline> Pipeline { get; set; }
        
        public static MapConfig Current  {get; private set; }

        public char DefaultDecimalSeparator { get; set; } = '.';
        
        public string DefaultCsvDelimiter { get; set; } = ";";
        public string SourceBaseDir { get; set; }
        
        public TraceMode TraceMode { get; set; }
        public static IEnumerable<Type> CustomTypes { get; set; } = new List<Type>();

        internal void Initialize()
        {
            Current = this;
            Pipeline.ToList().ForEach(i => i.Initialize());
        }

        public static MapConfig ReadFromFile(string yamlFilePath, IEnumerable<Type> customCommands, Func<Type, object> objectFactory = null)
        {
            var yamlString = File.ReadAllText(yamlFilePath);
            
            var yamlTokens = GetInternalCommands()
                .Union(customCommands ??  new List<Type>())
                .Select(type => (Token: CommandUtils.GetCommandYamlName(type), Type: type ))
                .ToList();
            

            var builder = new DeserializerBuilder();
            yamlTokens.ForEach(type => builder = builder.WithTagMapping("!" + type.Token, type.Type));

            builder.WithObjectFactory(type =>
            {
                return objectFactory?.Invoke(type) ?? new DefaultObjectFactory().Create(type);
            });
            

            var deserializer = builder.Build();
            var mapConfig = deserializer.Deserialize<MapConfig>(yamlString);
            mapConfig.Initialize();
            return mapConfig;
        }

        private static IEnumerable<Type> GetInternalCommands()
        {
            return new[]
            {
                typeof(SetFlowCommand),
                typeof(LookupCommand),
                typeof(ReplaceCommandSet),
                typeof(TypeConvertCommand),
                typeof(SetCommand),
                typeof(ConcatCommand),
                typeof(GetValueCommand),
                typeof(GetNotEmptyValueCommand),
                typeof(IfCommand),
                typeof(TraceCommand),
                typeof(CommandSet<CommandBase>),
                typeof(GetTargetCommand),
                typeof(MessageCommand),
                // typeof(CsvDataSource),
                // typeof(CompositeDataSource),
                // typeof(ExcelDataSource),
                // typeof(SqlDataSource),
            };
        }
    }
}