using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace XQ.DataMigration.Data
{
    public class CachedCsvDataSet : CachedDataSet
    {
        private readonly string _fileName;
        private readonly string _delimiter;

        private TextReader _txtReader;
        private CsvReader _csvReader;

        public CachedCsvDataSet(string fileName, string delimiter) : base(fileName)
        {
            _fileName = fileName;
            _delimiter = delimiter;
        }

        public override IValuesObject CreateObject()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator<IValuesObject> GetEnumerator()
        {
            Initialize();

            using (_txtReader)
            {
                using (_csvReader)
                {
                    while (_csvReader.Read())
                    {
                        ValuesObject result = new ValuesObject();
                        
                        _csvReader.FieldHeaders.ToList().ForEach(header =>
                        {
                            result.SetValue(header, _csvReader[header]);
                        });

                        yield return result;
                    }
                }
            }

        }

        private void Initialize()
        {
            this._txtReader = new StreamReader(_fileName, Encoding.GetEncoding("Windows-1252"));
            this._csvReader = new CsvReader(_txtReader);
            this._csvReader.Configuration.Delimiter = _delimiter;
            this._csvReader.Configuration.Encoding = Encoding.GetEncoding("Windows-1252");
        }
    }
}