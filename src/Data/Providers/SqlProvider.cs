using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class SqlProvider : TransitionNode, IDataProvider
    {
        [XmlAttribute]
        public string ConnectionString { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        [XmlAttribute] 
        public string Query { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<IValuesObject> GetDataSet(string queryString)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(queryString, connection);
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

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var actualQuery = Query.Contains("{") ? (string)ExpressionEvaluator.Evaluate(Query, ctx) : Query;
            ConnectionString = ConnectionString.Contains("{") ? (string)ExpressionEvaluator.Evaluate(ConnectionString, ctx) : ConnectionString;
            return new TransitResult(GetDataSet(actualQuery));
        }
    }
}