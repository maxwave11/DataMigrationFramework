using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Pipeline.Expressions;

namespace XQ.DataMigration
{
    public class MapConfig
    {
        private Dictionary<string, object> _variableValues { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
        
        public List<DataPipeline> Pipeline { get; set; } = new List<DataPipeline>();
        
        public Dictionary<string, MigrationExpression> _examples { get; set; }
        
        public static MapConfig Current  {get; private set; }

        public char DefaultDecimalSeparator { get; set; } = '.';
        
        public string DefaultCsvDelimiter { get; set; } = ";";
        public string SourceBaseDir { get; set; }
        
        public TraceMode TraceMode { get; set; }

        internal void Initialize()
        {
            Current = this;
            Pipeline.ForEach(i => i.Initialize());
        }
    }
}