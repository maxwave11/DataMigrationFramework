using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using XQ.DataMigration.Data;

namespace XQ.DataMigration.Data
{
    public class CsvDataSet : IDataSet
    {
        public string DataSetId { get; }
        public string Delimiter { get; }

        public CsvDataSet(string fileName, string delimiter)
        {
            DataSetId = fileName;
            Delimiter = delimiter;
        }

        public IEnumerator<IValuesObject> GetEnumerator()
        {
            var txtReader = new StreamReader(DataSetId, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = Delimiter;
            csvReader.Configuration.Encoding = Encoding.GetEncoding("Windows-1252");
            csvReader.Configuration.TrimFields = true;
            csvReader.Configuration.IgnoreBlankLines = true;

            using (txtReader)
            {
                using (csvReader)
                {
                    while (csvReader.Read())
                    {
                        ValuesObject result = new ValuesObject();
                        csvReader.FieldHeaders.ToList().ForEach(i =>
                        {
                            result.SetValue(i, csvReader[i]);
                        });

                        yield return result;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IValuesObject GetObjectByKey(string objectKey, Func<IValuesObject, string> evaluateKey)
        {
            //TransitLogger.LogInfo("WARNING: Used not cached CSV provider for lookup! There possible slow lookup performance.");
            var objects = this.ToList();
            return objects.SingleOrDefault(i => evaluateKey(i)?.ToUpper().Trim() == objectKey.ToUpper().Trim());
        }

        public IValuesObject GetObjectByExpression(string valueToFind, Func<IValuesObject, string> evaluateExpression, Func<IValuesObject, string> evaluateKey)
        {
            return GetObjectByKey(valueToFind, evaluateKey);
        }
    }
}