using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ExcelDataSource : TransitionNode, IDataSource
    {
        [XmlAttribute]
        public string DBPath { get; set; }

        /// <summary>
        /// Indicates in which row in file actual header located
        /// </summary>
        [XmlAttribute]
        public int HeaderRowNumber { get; set; } = 1;

        /// <summary>
        /// Indicates where in file actual data starts from
        /// </summary>
        [XmlAttribute]
        public int DataStartRowNumber { get; set; } = 2;

        [XmlAttribute]
        public string Query { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        public IEnumerable<IValuesObject> GetDataSet(string fileRelativePath)
        {
            var filePath = $"{DBPath}\\{fileRelativePath}";

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    string[] headerRow = null;
                    int rowCounter = 0;
                    while (reader.Read())
                    {
                        rowCounter++;

                        //init header row
                        if (rowCounter == HeaderRowNumber)
                        {
                            headerRow = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                                headerRow[i] = reader.GetString(i)?.Replace("\n", " ").Replace("\r", String.Empty);

                            continue;
                        }

                        if (rowCounter < DataStartRowNumber)
                            continue;

                        if (IsRowEmpty(reader))
                            continue;

                        yield return GetObjectFromRow(reader, headerRow);
                    }
                }
            }
        }

        private bool IsRowEmpty(IExcelDataReader reader)
        {
            bool result = true;

            for (int i = 0; i < reader.FieldCount; i++)
                result &= string.IsNullOrWhiteSpace(reader.GetValue(i)?.ToString());

            return result;
        }

        private IValuesObject GetObjectFromRow(IExcelDataReader reader, string [] headerRow)
        {
            //fill VlauesObject from row values
            var valuesObject = new ValuesObject();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (headerRow[i].IsEmpty())
                    continue;

                valuesObject.SetValue(headerRow[i], reader.GetValue(i));
            }

            return valuesObject;
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var actualQuery = ExpressionEvaluator.EvaluateString(Query, ctx);
            DBPath = ExpressionEvaluator.EvaluateString(DBPath, ctx);
            return new TransitResult(GetDataSet(actualQuery));
        }
    }
}