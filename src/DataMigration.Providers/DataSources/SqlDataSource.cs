using System.Data;
using System.Data.SqlClient;
using DataMigration.Data.Interfaces;

namespace DataMigration.Data.DataSources
{
    public class SqlDataSource : IDataSource<DefaultDataObject>
    {
        public string ConnectionString { get; set; }
        
        public string Query { get; set; }
        
        public Func<DefaultDataObject, string> Key { get; set; }
        
        public string GetObjectKey(DefaultDataObject dataObject) => Key(dataObject);
        
        public IEnumerable<DefaultDataObject> GetData()
        {
            var str = ConnectionString;
            using var connection = new SqlConnection(str);
            var adapter = new SqlDataAdapter();
           
            adapter.SelectCommand = new SqlCommand(Query, connection);
            var dataset = new DataSet();
            adapter.Fill(dataset);
            using DataTableReader reader = dataset.CreateDataReader();
            
            while (reader.Read())
            {
                var result = new DefaultDataObject();
                for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                {
                    result.SetValue(reader.GetName(fieldIndex), reader.GetValue(fieldIndex));
                }

                yield return result;
            }
        }
    }
}