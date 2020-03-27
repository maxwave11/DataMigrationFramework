﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;

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
            var actualQuery =  ExpressionEvaluator.EvaluateString(Query, ctx);
            ConnectionString = ExpressionEvaluator.EvaluateString(ConnectionString, ctx);
            return new TransitResult(GetDataSet(actualQuery));
        }
    }
}