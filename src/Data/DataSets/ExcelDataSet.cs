using ExcelDataReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ExcelDataSet : IDataSet
    {
        public string DataSetId { get; }
        public int HeaderRowNumber { get; }

        public ExcelDataSet(string fileName, int headerRowNumber)
        {
            DataSetId = fileName;
            HeaderRowNumber = headerRowNumber;
        }

        public IEnumerator<IValuesObject> GetEnumerator()
        {
            using (var stream = File.Open(DataSetId, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    string[] headerRow = null;
                    int rowCounter = 0;
                    while (reader.Read())
                    {
                        rowCounter++;

                        if (rowCounter < HeaderRowNumber)
                            continue;
                       
                        //init header row
                        if (headerRow == null)
                        {
                            headerRow = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                                headerRow[i] = reader.GetString(i)?.Replace("\n", " ").Replace("\r", String.Empty);
                            continue;
                        }

                        //skip empty rows
                        bool isEmptyRow = true;
                        for (int i = 0; i < reader.FieldCount; i++)
                            isEmptyRow &= string.IsNullOrWhiteSpace(reader.GetValue(i)?.ToString());

                        if (isEmptyRow)
                            continue;


                        //fill VlauesObject from row values
                        var valuesObject = new ValuesObject();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (headerRow[i].IsEmpty())
                                continue;

                            valuesObject.SetValue(headerRow[i], reader.GetValue(i));
                        }

                        yield return valuesObject;
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
            //TransitLogger.LogInfo("WARNING: Used not cached EXCEL provider for lookup! There possible slow lookup performance.");
            var objects = this.ToList();
            return objects.SingleOrDefault(i => evaluateKey(i)?.ToUpper().Trim() == objectKey.ToUpper().Trim());
        }

        public IValuesObject GetObjectByExpression(string valueToFind, Func<IValuesObject, string> evaluateExpression, Func<IValuesObject, string> evaluateKey)
        {
            return GetObjectByKey(valueToFind, evaluateKey);
        }
    }
}