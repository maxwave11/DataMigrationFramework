﻿using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ExcelProvider : ISourceProvider
    {
        [XmlAttribute]
        public string DBPath { get; set; }

        [XmlAttribute]
        public string DefaultQuery { get; set; }

        [XmlAttribute]
        public int HeaderRowNumber { get; set; } = 0;

        [XmlAttribute]
        public string Name { get; set; }
        public string Query { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        public IEnumerable<IValuesObject> GetDataSet(string providerQuery)
        {
            var path = DBPath + "\\" + (providerQuery.IsNotEmpty() ? providerQuery : DefaultQuery);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    string[] headerRow = null;
                    int rowCounter = 0;
                    while (reader.Read())
                    {
                        rowCounter++;

                        if (rowCounter < HeaderRowNumber)
                            continue;

                        //init header row
                        if (headerRow == null)
                        {
                            headerRow = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                                headerRow[i] = reader.GetString(i)?.Replace("\n", " ").Replace("\r", String.Empty);
                            continue;
                        }

                        //skip empty rows
                        bool isEmptyRow = true;
                        for (int i = 0; i < reader.FieldCount; i++)
                            isEmptyRow &= string.IsNullOrWhiteSpace(reader.GetValue(i)?.ToString());

                        if (isEmptyRow)
                            continue;


                        //fill VlauesObject from row values
                        var valuesObject = new ValuesObject();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (headerRow[i].IsEmpty())
                                continue;

                            valuesObject.SetValue(headerRow[i], reader.GetValue(i));
                        }

                        yield return valuesObject;
                    }
                }
            }
        }

        public TransitResult Transit(ValueTransitContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}