using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;

namespace XQ.DataMigration.MapConfig
{
    public class MapConfig
    {
        [XmlArray(nameof(TransitionGroups))]
        [XmlArrayItem(nameof(TransitionGroup))]
        public List<TransitionGroup> TransitionGroups { get; set; }

        [XmlArray(nameof(DataProviders))]
        public List<Object> DataProviders { get; set; }

        public MapConfig()
        {
        }

        internal void Initialize()
        { 
            DataProviders.ForEach(p=>((IDataProvider)p).Initialize());
            TransitionGroups?.ForEach(i => i.Initialize(null));
        }

        public ISourceProvider GetDefaultSourceProvider()
        {
            return DataProviders.OfType<ISourceProvider>().Single(i => i.IsDefault);
        }

        public ITargetProvider GetDefaultTargetProvider()
        {
            return DataProviders.OfType<ITargetProvider>().Single(i => i.IsDefault);
        }

        public ITargetProvider GetTargetProvider(string targetProviderName)
        {
            return DataProviders.OfType<ITargetProvider>().SingleOrDefault(i => i.Name == targetProviderName);
        }

        public ISourceProvider GetSourceProvider(string sourceProviderName)
        {
            return DataProviders.OfType<ISourceProvider>().SingleOrDefault(i => i.Name == sourceProviderName);
        }

        public IDataProvider GetDataProvider(string providerName)
        {
            return DataProviders.OfType<IDataProvider>().SingleOrDefault(i => i.Name == providerName);
        }
    }
}