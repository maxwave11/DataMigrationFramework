using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data.DataSources
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

    public class CompositeDataSource : List<IDataSource>, IDataSource
    {
        public IEnumerable<IValuesObject> GetData()
        {
            return this.SelectMany(i => i.GetData());
        }

        public IEnumerable<IValuesObject> GetObjectsByKey(string key)
        {
            throw new NotImplementedException();
        }
    }

    public class CsvDataSource : DataSourceBase
    {
        protected override IEnumerable<IValuesObject> GetDataInternal()
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();

            var subQueries = Query.Split('|');
            foreach (string subQuery in subQueries)
            {
                var filePath = $"{settings.Path}\\{subQuery.Trim()}";

                foreach (var valuesObject in GetDataFromFile(filePath))
                {
                    yield return valuesObject;
                }
            }
        }

        private IEnumerable<IValuesObject> GetDataFromFile(string filePath)
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();

            var txtReader = new StreamReader(filePath, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = settings.Delimiter;
            csvReader.Configuration.Encoding = Encoding.GetEncoding("Windows-1252");
            csvReader.Configuration.TrimFields = true;
            csvReader.Configuration.IgnoreBlankLines = true;
            csvReader.Configuration.TrimHeaders = true;

            Key.TraceColor = ConsoleColor.Green;
            
            using (txtReader)
            {
                using (csvReader)
                {
                    while (csvReader.Read())
                    {
                        var result = new ValuesObject();
                        csvReader.FieldHeaders.ToList().ForEach(i =>
                        {
                            if (i.IsNotEmpty())
                                result.SetValue(i.Trim(), csvReader[i.Trim()]);
                        });
                        
                        yield return result;
                    }
                }
            }
        }
    }

}