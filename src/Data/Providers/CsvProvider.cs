using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class CsvProvider : TransitionNode, ISourceProvider
    {
        [XmlAttribute]
        public string DBPath { get;  set; }

        [XmlAttribute]
        public string DefaultDataSetId { get; set; }

        [XmlAttribute]
        public string Delimiter { get; set; } = ";";

        [XmlAttribute]
        public bool CacheData { get; set; }

        [XmlAttribute]
        public string Query { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        public void Initialize()
        {
        }

        private readonly Dictionary<string, IDataSet> _dataSets = new Dictionary<string, IDataSet>();

        public IDataSet GetDataSet(string dataSetId)
        {
            if (!CacheData)
                return new CsvDataSet(DBPath + "\\" + (dataSetId.IsNotEmpty() ? dataSetId : DefaultDataSetId), Delimiter);

            if (!_dataSets.ContainsKey(dataSetId))
                _dataSets[dataSetId] = new CachedCsvDataSet(DBPath + "\\" + (dataSetId.IsNotEmpty() ? dataSetId : DefaultDataSetId), Delimiter);

            return _dataSets[dataSetId];
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var actualQuery = Query.Contains("{") ? (string)ExpressionEvaluator.Evaluate(Query, ctx) : Query;
            DBPath = DBPath.Contains("{") ? (string)ExpressionEvaluator.Evaluate(DBPath, ctx) : DBPath;
            return new TransitResult(GetDataSet(actualQuery));
        }
    }
}