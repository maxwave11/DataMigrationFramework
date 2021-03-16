using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataMigration.Utils;

namespace DataMigration.Data.DataSources
{
    public class CsvDataSource : DataSourceBase
    {
        public string Delimiter { get; set; }
        public string Encoding { get; set; } = "Windows-1252";

        protected override IEnumerable<IDataObject> GetDataInternal()
        {
            string baseDir = MapConfig.Current.SourceBaseDir;
            
            var subQueries =  ActualQuery.Split('|');
            foreach (string subQuery in subQueries)
            {
                string fullPath = $"{baseDir}\\{subQuery.Trim()}";
                string dirPath = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath);

                var files = Directory.GetFiles(dirPath, fileName).OrderBy(i=>i).ToList();

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
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var txtReader = new StreamReader(stream);

            var csvConfiguration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfiguration.Delimiter = Delimiter ?? MapConfig.Current.DefaultCsvDelimiter;
           // csvConfiguration.Encoding = System.Text.Encoding.GetEncoding(Encoding);
            csvConfiguration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
            csvConfiguration.IgnoreBlankLines = true;
            csvConfiguration.MissingFieldFound = null;
            //csvConfiguration.TrimHeaders = true;
            using var csvReader = new CsvReader(txtReader, csvConfiguration);

            Key.TraceColor = ConsoleColor.Green;

            csvReader.Read();
            csvReader.ReadHeader();


            while (csvReader.Read())
            {
               
                var result = new DataObject();

                csvReader.HeaderRecord.ToList().ForEach(header =>
                {
                    if (header.IsNotEmpty())
                        result.SetValue(header.Trim(), csvReader[header.Trim()]);
                });
                        
                yield return result;
            }
        }
    }

}