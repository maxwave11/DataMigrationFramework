using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DataMigration.Pipeline.Commands;

namespace DataMigration.Data.DataSources
{
    [Yaml("sql")]
    public class SqlDataSource : DataSourceBase<DefaultDataObject>
    {
        public string ConnectionString { get; set; }

        protected override IEnumerable<DefaultDataObject> GetDataInternal()
        {
            var str = ConnectionString ?? MapConfig.Current.Variables["ConnectionString"].ToString();

            using var connection = new SqlConnection(str);
            var adapter = new SqlDataAdapter();
           
            adapter.SelectCommand = new SqlCommand(ActualQuery, connection);
            var dataset = new DataSet();
            adapter.Fill(dataset);
            using DataTableReader reader = dataset.CreateDataReader();
            
            while (reader.Read())
            {
                DefaultDataObject result = new DefaultDataObject();
                for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                {
                    result.SetValue(reader.GetName(fieldIndex), reader.GetValue(fieldIndex));
                }

                yield return result;
            }
        }
    }
}