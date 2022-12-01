using CsvHelper;
using System.Globalization;
using DataMigration.Data.Interfaces;
using DataMigration.Utils;


namespace DataMigration.Data.DataSources
{
    public class CsvDataSource : IDataSource<DefaultDataObject>
    {
        public string Delimiter { get; set; }
        public string Encoding { get; set; } = "Windows-1252";
        public string Files { get; set; }
        public Func<DefaultDataObject, string> Key { get; set; }
        
        public string GetObjectKey(DefaultDataObject dataObject) => Key(dataObject);
        
        
        public IEnumerable<DefaultDataObject> GetData()
        {
            string baseDir = ".";
            
            var subQueries =  Files.Split('|');
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
                        yield return valuesObject;
                    }
                }
            }
        }
        
        private IEnumerable<DefaultDataObject> GetDataFromFile(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var txtReader = new StreamReader(stream);

            var csvConfiguration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfiguration.Delimiter = Delimiter;
            csvConfiguration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
            csvConfiguration.IgnoreBlankLines = true;
            csvConfiguration.MissingFieldFound = null;
            if (Encoding.IsNotEmpty())
                csvConfiguration.Encoding = System.Text.Encoding.GetEncoding(Encoding);
            //csvConfiguration.TrimHeaders = true;
            using var csvReader = new CsvReader(txtReader, csvConfiguration);

            csvReader.Read();
            csvReader.ReadHeader();
            
            while (csvReader.Read())
            {
                var result = new DefaultDataObject();

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