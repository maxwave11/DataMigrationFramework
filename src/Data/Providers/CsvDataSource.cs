using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
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
        
        public ReadKeyTransition Key { get; set; }
        
        // public KeyTransition ComplexKey { get; set; }

        public IEnumerable<IValuesObject> GetData()
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();


            var subQueries = Query.Split('|');
            foreach (string subQuery in subQueries)
            {
                var filePath = $"{settings.Path}\\{subQuery.Trim()}";

                foreach (var valuesObject in GetDataFromFile(filePath))
                {
                   var result =  Key.Transit(new ValueTransitContext(valuesObject,null, valuesObject));

                   valuesObject.Key = result.Value.ToString();
                   
                    if (valuesObject.Key.IsEmpty())
                        continue;
                    
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
            Key.Initialize(null);
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
    }

}