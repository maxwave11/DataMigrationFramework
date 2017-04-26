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
        public ISourceProvider SrcProvider { get; private set; }
        [XmlIgnore]
        public ITargetProvider TargetProvider { get; private set; }

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
        public string SourceProviderName { get; set; }

        [XmlAttribute]
        public string TargetProviderName { get; set; }

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
            if (this.SourceProviderName.IsEmpty())
                throw new Exception($"{ nameof(SourceProviderName)} is required");

            if (this.TargetProviderName.IsEmpty())
                throw new Exception($"{ nameof(TargetProviderName)} is required");

            SrcProvider = MapConfig.GetSourceProvider(SourceProviderName);
            TargetProvider = MapConfig.GetTargetProvider(TargetProviderName);
            TargetProvider.Initialize();
        }

        public void Dispose()
        {
            TargetProvider.Dispose();
        }
    }
}