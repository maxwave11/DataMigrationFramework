using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data.DataSources
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
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var txtReader = new StreamReader(stream, System.Text.Encoding.GetEncoding(Encoding));
            var csvReader = new CsvReader(txtReader);
            csvReader.Configuration.Delimiter = Delimiter ?? MapConfig.Current.DefaultCsvDelimiter;
            csvReader.Configuration.Encoding = System.Text.Encoding.GetEncoding(Encoding);
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