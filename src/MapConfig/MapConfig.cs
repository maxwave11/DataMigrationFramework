using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.MapConfig
{
    public class MapConfig
    {
        public List<IDataSourceSettings> SourceSettings { get; set; } = new List<IDataSourceSettings>();
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
        public List<TransitDataCommand> Pipeline { get; set; } = new List<TransitDataCommand>();


        internal void Initialize()
        { 
            Pipeline.ForEach(i => i.Initialize(null));
        }

        public T GetDefaultSourceSettings<T>()
        {
            return SourceSettings.OfType<T>().Single();
        }

        public ITargetProvider GetTargetProvider()
        {
            //Only one TargetProvider allowed at current moment!
            //return DataSources.OfType<ITargetProvider>().Single();
            return null;
        }
    }
}