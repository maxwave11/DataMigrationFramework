using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class CsvSourceSettings 
    {
        public string Path { get; set; }
        public string Delimiter { get; set; } = ";";
        public bool HasHeaderRow { get; set; }
        public bool IsDefault { get; set; }
    }

    public class CsvDataSource : DataSourceBase
    {
        public CsvSourceSettings Settings { get; set; }

        protected override IEnumerable<IValuesObject> GetDataInternal()
        {
            var settings = Settings ?? Migrator.Current.MapConfig.GetDefaultSourceSettings<CsvSourceSettings>();
            var filePath = $"{settings.Path}\\{Query}";

            var txtReader = new StreamReader(filePath, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = settings.Delimiter;
            csvReader.Configuration.Encoding = Encoding.GetEncoding("Windows-1252");
            csvReader.Configuration.TrimFields = true;
            csvReader.Configuration.IgnoreBlankLines = true;
            csvReader.Configuration.TrimHeaders = true;
            csvReader.Configuration.HasHeaderRecord = settings.HasHeaderRow;

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