using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    public class CsvDataSource : DataSourceBase
    {
        //public CsvSourceSettings Settings { get; set; }

        protected override IEnumerable<IValuesObject> GetDataInternal()
        {
            var settings = Migrator.Current.MapConfig.GetDefaultSourceSettings<CsvSourceSettings>();
            var filePath = $"{settings.Path}\\{Query}";

            var txtReader = new StreamReader(filePath, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = settings.Delimiter;
            csvReader.Configuration.Encoding = Encoding.GetEncoding("Windows-1252");
            csvReader.Configuration.TrimFields = true;
            csvReader.Configuration.IgnoreBlankLines = true;
            csvReader.Configuration.TrimHeaders = true;
            //csvReader.Configuration.HasHeaderRecord = settings.HasHeaderRowNUmber;

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
                                result.SetValue(i, csvReader[i]);
                        });

                        

                        yield return result;
                    }
                }
            }
        }
    }

}