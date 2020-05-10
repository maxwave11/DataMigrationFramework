using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;

namespace XQ.DataMigration.Data
{
    public class SqlDataSource : DataSourceBase 
    {
        public string ConnectionString { get; set; }

        protected override IEnumerable<IValuesObject> GetDataInternal()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(Query, connection);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);
                using (DataTableReader reader = dataset.CreateDataReader())
                {
                    while (reader.Read())
                    {
                        ValuesObject result = new ValuesObject();
                        for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                        {
                            result.SetValue(reader.GetName(fieldIndex), reader.GetValue(fieldIndex));
                        }

                        yield return result;
                    }
                }
            }
        }
    }
}