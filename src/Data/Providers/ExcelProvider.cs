using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ExcelProvider : ISourceProvider
    {
        [XmlAttribute]
        public string DBPath { get; set; }

        [XmlAttribute]
        public string DefaultDataSetId { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
        public string Query { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        public void Initialize()
        {
        }

        public IDataSet GetDataSet(string providerQuery)
        {
            var path = DBPath + "\\" + (providerQuery.IsNotEmpty() ? providerQuery : DefaultDataSetId);

            return new ExcelDataSet(path);
        }

        public TransitResult Transit(ValueTransitContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}