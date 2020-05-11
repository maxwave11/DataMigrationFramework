using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ExcelDataSource : DataSourceBase
    {
        public IDataSourceSettings Settings { get; set; }

        protected override IEnumerable<IValuesObject> GetDataInternal()
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();
            Settings = settings;
            var filePath = $"{settings.Path}\\{Query}";
            
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
                        if (rowCounter == settings.HeaderRowNumber)
                        {
                            headerRow = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                                headerRow[i] = reader.GetString(i)?.Replace("\n", " ").Replace("\r", String.Empty);
            
                            continue;
                        }
            
                        if (rowCounter < settings.DataStartRowNumber)
                            continue;
            
                        if (IsRowEmpty(reader))
                            continue;
            
                        yield return RowToValuesObject(reader, headerRow);
                    }
                }
            }
        }
        
        private IValuesObject RowToValuesObject(IExcelDataReader reader, string [] headerRow)
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
        
        private bool IsRowEmpty(IExcelDataReader reader)
        {
            bool result = true;

            for (int i = 0; i < reader.FieldCount; i++)
                result &= string.IsNullOrWhiteSpace(reader.GetValue(i)?.ToString());

            return result;
        }
    }
}