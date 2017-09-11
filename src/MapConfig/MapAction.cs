using System;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.MapConfig
{
    public class MapAction: IDisposable
    {
        [XmlIgnore]
        public ISourceProvider DefaultSourceProvider { get; private set; }
        [XmlIgnore]
        public ITargetProvider DefaultTargetProvider { get; private set; }

        public MapAction()
        {
            this.DoMapping = true;
            this.DoSave = true;
        }

        [XmlAttribute]
        public bool DoMapping { get; set; }

        [XmlAttribute]
        public bool DoSave { get; set; }

        [XmlElement]
        public string Filter { get; set; }

        [XmlAttribute]
        public string ForceMappingFrom { get; set; }

        [XmlAttribute]
        public string ExcludeFromMapping { get; set; }

        [XmlAttribute]
        public string ForceRowsRange { get; set; }

        [XmlAttribute]
        public string MapDataBaseName { get; set; }

        [XmlAttribute]
        public string DefaultSourceProviderName { get; set; }

        [XmlAttribute]
        public string DefaultTargetProviderName { get; set; }

        public string TransitionGroupName { get; set; }
   
        public MapConfig MapConfig { get; set; }

        public string GetFilterValue(string key)
        {
            if (this.Filter.IsEmpty())
                return null;
            return this.Filter.Split(',').First(i => i.StartsWith(key)).Split('=')[1];
        }

        public void Initialize()
        {
            if (this.DefaultSourceProviderName.IsEmpty())
                throw new Exception($"{ nameof(DefaultSourceProviderName)} is required");

            if (this.DefaultTargetProviderName.IsEmpty())
                throw new Exception($"{ nameof(DefaultTargetProviderName)} is required");

            DefaultSourceProvider = MapConfig.GetSourceProvider(DefaultSourceProviderName);
            DefaultTargetProvider = MapConfig.GetTargetProvider(DefaultTargetProviderName);
            DefaultTargetProvider.Initialize();
        }

        public void Dispose()
        {
            DefaultTargetProvider.Dispose();
        }
    }
}