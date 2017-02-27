using System.Xml.Serialization;
using XQ.DataMigration.Data;

namespace XQ.DataMigration.MapConfig
{
    public class DataProviderSettings
    {
        [XmlAttribute]
        public string Name { get; set; }
    
        [XmlElement]
        public object DataProvider { get; set; }

        public void Initialize()
        {
            (DataProvider as ITargetProvider)?.Initialize();
        }
    }
}