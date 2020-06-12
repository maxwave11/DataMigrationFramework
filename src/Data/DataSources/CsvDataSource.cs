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
    public class CsvDataSource : DataSourceBase
    {
        public string Delimiter { get; set; }

        protected override IEnumerable<IDataObject> GetDataInternal()
        {
            //var settings = MapConfig.Current.GetDefaultSourceSettings<CsvSourceSettings>();
            var baseDir = MapConfig.Current.SourceBaseDir;
   
            
            var subQueries =  ActualQuery.Split('|');
            foreach (string subQuery in subQueries)
            {
                var fullPath = $"{baseDir}\\{subQuery.Trim()}";
                var dirPath = Path.GetDirectoryName(fullPath);
                var fileName = Path.GetFileName(fullPath);

                var files = Directory.GetFiles(dirPath, fileName).OrderBy(i=>i);

                if (!files.Any())
                    throw new FileNotFoundException($"There is no {fileName} files in {dirPath}");

                foreach (string file in files)
                {
                    foreach (var valuesObject in GetDataFromFile(file))
                    {
                        //let migration logic know from which source file value comes
                        valuesObject.Query = file;
                        yield return valuesObject;
                    }
                }
            }
        }

        private IEnumerable<IDataObject> GetDataFromFile(string filePath)
        {
            Migrator.Current.Tracer.TraceLine("Reading " + filePath);
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var txtReader = new StreamReader(stream, Encoding.GetEncoding("Windows-1252"));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = Delimiter.IsEmpty() ? MapConfig.Current.DefaultCsvDelimiter : Delimiter;
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
                        var result = new DataObject();
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