using DataMigration.Data.Interfaces;
using ExcelDataReader;
using DataMigration.Utils;

namespace DataMigration.Data.DataSources
{
    public class ExcelDataSource : IDataSource<DefaultDataObject>
    {
        public string Files { get; set; }
        private DefaultDataObject RowToValuesObject(IExcelDataReader reader, string [] headerRow)
        {
            //fill VlauesObject from row values
            var valuesObject = new DefaultDataObject();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (headerRow[i].IsEmpty())
                    continue;

                valuesObject.SetValue(headerRow[i], reader.GetValue(i));
            }

            return valuesObject;
        }
        
        private bool IsRowEmpty(IExcelDataReader reader)
        {
            bool result = true;

            for (int i = 0; i < reader.FieldCount; i++)
                result &= string.IsNullOrWhiteSpace(reader.GetValue(i)?.ToString());

            return result;
        }

        public string GetObjectKey(DefaultDataObject dataObject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DefaultDataObject> GetData()
        {
            var baseDir = '.';

            var filePath = $"{baseDir}\\{Files}";
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            
            using var reader = ExcelReaderFactory.CreateReader(stream);

            string[] headerRow = null;
            int rowCounter = 0;
            while (reader.Read())
            {
                rowCounter++;
            
                //init header row
                if (rowCounter == 1)
                {
                    headerRow = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                        headerRow[i] = reader.GetString(i)?.Replace("\n", " ").Replace("\r", String.Empty);
            
                    continue;
                }
            
                if (rowCounter < 2)
                    continue;
            
                if (IsRowEmpty(reader))
                    continue;
                        
                var valuesObject = RowToValuesObject(reader, headerRow);
                yield return valuesObject;
            }
        }
    }
}