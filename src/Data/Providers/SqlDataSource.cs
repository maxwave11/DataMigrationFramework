using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;

namespace XQ.DataMigration.Data
{
    public class SqlDataSource : IDataSource //INHERIT FROM DataSourceBase
    {
        public string ConnectionString { get; set; }

        public bool IsDefault { get; set; }

        public string Query { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

       

        public IEnumerable<IValuesObject> GetData()
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

        //public override TransitResult TransitInternal(ValueTransitContext ctx)
        //{
        //    var actualQuery =  ExpressionEvaluator.EvaluateString(Query, ctx);
        //    ConnectionString = ExpressionEvaluator.EvaluateString(ConnectionString, ctx);
        //    return new TransitResult(GetDataSet(actualQuery));
        //}
    }
}