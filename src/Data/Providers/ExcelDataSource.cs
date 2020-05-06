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
        public override IEnumerable<IValuesObject> GetData()
        {
            throw new NotImplementedException();
        }

        protected override IDataReader GetDataReader()
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();
            Settings = settings;
            var filePath = $"{settings.Path}\\{Query}";
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var reader = ExcelReaderFactory.CreateReader(stream);
            return reader;
            //
            // using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            // {
            //     using (var reader = ExcelReaderFactory.CreateReader(stream))
            //     {
            //         string[] headerRow = null;
            //         int rowCounter = 0;
            //         while (reader.Read())
            //         {
            //             rowCounter++;
            //
            //             //init header row
            //             if (rowCounter == settings.HeaderRowNumber)
            //             {
            //                 headerRow = new string[reader.FieldCount];
            //                 for (int i = 0; i < reader.FieldCount; i++)
            //                     headerRow[i] = reader.GetString(i)?.Replace("\n", " ").Replace("\r", String.Empty);
            //
            //                 continue;
            //             }
            //
            //             if (rowCounter < settings.DataStartRowNumber)
            //                 continue;
            //
            //             if (IsRowEmpty(reader))
            //                 continue;
            //
            //             yield return RowToValuesObject(reader, headerRow);
            //         }
            //     }
            // }
        }
    }
}