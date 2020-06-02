using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
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

    public class CsvDataSource : DataSourceBase
    {
        public ExpressionCommand<string> ExprQuery { get; set; }

        public string Delimiter { get; set; }

        protected override IEnumerable<IValuesObject> GetDataInternal()
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();
            if (ExprQuery != null)
            {
                var ctx = new ValueTransitContext(null,null);
                Query = ctx.Execute(ExprQuery);
            }
            
            var subQueries =  Query.Split('|');
            foreach (string subQuery in subQueries)
            {
                var fullPath = $"{settings.Path}\\{subQuery.Trim()}";
                var dirPath = Path.GetDirectoryName(fullPath);
                var fileName = Path.GetFileName(fullPath);

                var files = Directory.GetFiles(dirPath, fileName).OrderBy(i=>i);

                if (!files.Any())
                    throw new FileNotFoundException($"There is no {fileName} files in {dirPath}");

                foreach (string file in files)
                {
                    foreach (var valuesObject in GetDataFromFile(file))
                    {
                        yield return valuesObject;
                    }
                }
            }
        }

        private IEnumerable<IValuesObject> GetDataFromFile(string filePath)
        {
            var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();
            Migrator.Current.Tracer.TraceLine("Reading " + filePath);
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var txtReader = new StreamReader(stream, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = Delimiter.IsEmpty() ? settings.Delimiter : Delimiter;
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