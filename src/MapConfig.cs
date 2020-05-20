using System;
using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Pipeline.Expressions;

namespace XQ.DataMigration
{
    public class MapConfig
    {
        public List<IDataSourceSettings> SourceSettings { get; set; } = new List<IDataSourceSettings>();
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
        public List<DataPipeline> Pipeline { get; set; } = new List<DataPipeline>();
        public Dictionary<string, MigrationExpression> _examples { get; set; }
        
        public static MapConfig Current  {get; private set; }

        public char DefaultDecimalSeparator { get; set; } = '.';
        
        public TraceMode TraceMode { get; set; }

        internal void Initialize()
        {
            Current = this;
            Pipeline.ForEach(i => i.Initialize());
        }
        
        public T GetDefaultSourceSettings<T>()
        {
            return SourceSettings.OfType<T>().Single();
        }
    }

    [Flags]
    public enum TraceMode
    {
        Auto         = 0,                  // 000000
        Objects        = 1,           // 000001
        Commands         = 3,           // 000011
    }
}