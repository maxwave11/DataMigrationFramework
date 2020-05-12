using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.MapConfiguration
{
    public class MapConfig
    {

        public List<IDataSourceSettings> SourceSettings { get; set; } = new List<IDataSourceSettings>();
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
        public List<TransitDataCommand> Pipeline { get; set; } = new List<TransitDataCommand>();
        public Dictionary<string, MigrationExpression> _examples { get; set; }
        
        public static MapConfig Current  {get; private set; }

        public char DefaultDecimalSeparator { get; set; } = '.';
        public bool TraceKeyTransition { get; set; }
        public bool TraceValueTransition { get; set; }

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
}