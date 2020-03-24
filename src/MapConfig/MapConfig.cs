using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes;

namespace XQ.DataMigration.MapConfig
{
    public class MapConfig
    {
        /// <summary>
        /// List of nested transitions. 
        /// </summary>
        public List<TransitionNode> ChildTransitions { get; set; }

        [XmlArray(nameof(DataProviders))]
        public List<Object> DataProviders { get; set; }

        public MapConfig()
        {
        }

        internal void Initialize()
        { 
            ChildTransitions?.ForEach(i => i.Initialize(null));
        }

        public IDataProvider GetDefaultDataProvider()
        {
            if (!DataProviders.OfType<IDataProvider>().Any(i => i.IsDefault))
                throw new InvalidOperationException("Can't find default source data provider");

            return DataProviders.OfType<IDataProvider>().Single(i => i.IsDefault);
        }

        public IDataProvider GetDataProvider(string providerName)
        {
            return DataProviders.OfType<IDataProvider>().SingleOrDefault(i => i.Name == providerName);
        }

        public ITargetProvider GetTargetProvider()
        {
            //Only one TargetProvider allowed at current moment!
            return DataProviders.OfType<ITargetProvider>().Single();
        }

        //public ITargetProvider GetTargetProvider(string targetProviderName)
        //{
        //    return DataProviders.OfType<ITargetProvider>().SingleOrDefault(i => i.Name == targetProviderName);
        //}

        //public IDataProvider GetDataProvider(string sourceProviderName)
        //{
        //    return DataProviders.OfType<IDataProvider>().SingleOrDefault(i => i.Name == sourceProviderName);
        //}

      
    }
}