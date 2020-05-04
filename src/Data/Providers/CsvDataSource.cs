using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class CsvSourceSettings: IDataSourceSettings
    {
        public string Path { get; set; }
        public string Delimiter { get; set; } = ";";

        /// <summary>
        /// Indicates in which row in file actual header located
        /// </summary>
        public int HeaderRowNumber { get; set; } = 1;
        
        public int DataStartRowNumber { get; set; } = 2;
    }

    public class CsvDataSource : IDataSource
    {
        public string Query { get; set; }
        
        public KeyTransition Key { get; set; }

        public IEnumerable<IValuesObject> GetData()
        {
            var settings = Migrator.Current.MapConfig.GetDefaultSourceSettings<CsvSourceSettings>();
            string filePath = $"{settings.Path}\\{Query}";
            var txtReader = new StreamReader(filePath, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = settings.Delimiter;
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
                            if (i.IsNotEmpty())
                                result.SetValue(i, csvReader[i]);
                        });
                        
                        result.Key = Key.GetKeyForObject(result);

                        if (result.Key.IsEmpty())
                            continue;

                        yield return result;
                    }
                }
            }
        }
    }

}