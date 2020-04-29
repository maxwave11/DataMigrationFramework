using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

        protected override IDataReader GetDataReader()
        {
            var settings = Migrator.Current.MapConfig.GetDefaultSourceSettings<CsvSourceSettings>();
            string filePath = $"{settings.Path}\\{Query}";

            var txtReader = new StreamReader(filePath, Encoding.GetEncoding("Windows-1252"));
            
            var readerConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = settings.Delimiter,
                Encoding = Encoding.GetEncoding("Windows-1252"),
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                PrepareHeaderForMatch = (header,i)=> header.Trim()
            };
            
            var csvReader = new CsvDataReader(new CsvReader(txtReader,readerConfiguration));

            return csvReader;

            // using (txtReader)
            // {
            //     using (csvReader)
            //     {
            //         while (csvReader.Read())
            //         {
            //             var result = new ValuesObject();
            //             csvReader.FieldHeaders.ToList().ForEach(i =>
            //             {
            //                 if (i.IsNotEmpty())
            //                     result.SetValue(i, csvReader[i]);
            //             });
            //
            //             yield return result;
            //         }
            //     }
            // }
        }
    }

}