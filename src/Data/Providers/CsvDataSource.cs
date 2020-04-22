using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class CsvDataSource : TransitionNode, IDataSource
    {
        [XmlAttribute]
        public string DBPath { get;  set; }

        [XmlAttribute]
        public string Delimiter { get; set; } = ";";

        [XmlAttribute]
        public string Query { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        [XmlAttribute]
        public bool HasHeaderRow { get; set; }

        public IEnumerable<IValuesObject> GetDataSet(string fileRelativePath)
        {
            var filePath = $"{DBPath}\\{fileRelativePath}";

            var txtReader = new StreamReader(filePath, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = Delimiter;
            csvReader.Configuration.Encoding = Encoding.GetEncoding("Windows-1252");
            csvReader.Configuration.TrimFields = true;
            csvReader.Configuration.IgnoreBlankLines = true;
            csvReader.Configuration.TrimHeaders = true;
            csvReader.Configuration.HasHeaderRecord = HasHeaderRow;

            using (txtReader)
            {
                using (csvReader)
                {
                    while (csvReader.Read())
                    {
                        ValuesObject result = new ValuesObject();
                        csvReader.FieldHeaders.ToList().ForEach(i =>
                        {
                            if (i.IsNotEmpty())
                                result.SetValue(i, csvReader[i]);
                        });

                        yield return result;
                    }
                }
            }

        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var actualQuery = ExpressionEvaluator.EvaluateString(Query, ctx);
            DBPath = ExpressionEvaluator.EvaluateString(DBPath, ctx);
            return new TransitResult(GetDataSet(actualQuery));
        }
    }
}