using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;

namespace XQ.DataMigration.MapConfig
{
    public class MapConfig
    {
        [XmlArray(nameof(TransitionGroups))]
        [XmlArrayItem(nameof(TransitionGroup))]
        public List<TransitionGroup> TransitionGroups { get; set; }

        [XmlArray(nameof(MapActions))]
        [XmlArrayItem(nameof(MapAction))]
        public List<MapAction> MapActions { get; set; }

        [XmlArray(nameof(MapProviders))]
        public List<DataProviderSettings> MapProviders { get; set; }

        public MapConfig()
        {
            this.MapActions = new List<MapAction>();
        }

        internal void Initialize()
        { 
            MapProviders.ForEach(p=>p.Initialize());

            MapActions.ForEach(act => act.MapConfig = this);

            TransitionGroups?.ForEach(i => i.Initialize(null));
        }

        public ISourceProvider GetSourceProvider(string providerName)
        {
            return (ISourceProvider)MapProviders.SingleOrDefault(i => i.Name == providerName)?.DataProvider;
        }

        public ITargetProvider GetTargetProvider(string providerName)
        {
            return (ITargetProvider)MapProviders.SingleOrDefault(i => i.Name == providerName)?.DataProvider;
        }

        public IDataProvider GetDataProvider(string providerName)
        {
            return (IDataProvider)MapProviders.SingleOrDefault(i => i.Name == providerName)?.DataProvider;
        }
    }
}