using System;
using System.Collections.Generic;
using System.Data;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public abstract class DataSourceBase : IDataSource
    {
        public string Query { get; set; }

        public KeyTransition Key { get; set; }

        public IDataSourceSettings Settings { get; set; }

        /// <summary>
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        public string RowsRange { get; set; }

        //private Dictionary<int, int> _allowedRanges;

        protected abstract IDataReader GetDataReader();

        public IEnumerable<IValuesObject> GetData()
        {
            var reader = GetDataReader();


            using (reader)
            {
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
                
                    yield return RowToValuesObject(reader, headerRow);
                }
            }
        }
        
        private bool IsRowEmpty(IDataReader reader)
        {
            bool result = true;

            for (int i = 0; i < reader.FieldCount; i++)
                result &= string.IsNullOrWhiteSpace(reader.GetValue(i)?.ToString());

            return result;
        }
        
        private IValuesObject RowToValuesObject(IDataReader reader, string[] headerRow)
        {
            //fill VlauesObject from row values
            var valuesObject = new ValuesObject();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (headerRow[i].IsEmpty())
                    continue;

                valuesObject.SetValue(headerRow[i], reader.GetValue(i));
            }

            valuesObject.Key = Key.GetKeyForObject(valuesObject);

            if (valuesObject.Key.IsEmpty())
                throw new InvalidOperationException("Object key can't be empty");

            return valuesObject;
        }
        

        //private bool IsRowIndexInRange(int rowIndex)
        //{
        //    if (RowsRange.IsEmpty()) return true;

        //    return _allowedRanges.Any(i => i.Key <= rowIndex && rowIndex <= i.Value);
        //}

        //private void ParseRowsRange()
        //{
        //    if (RowsRange.IsEmpty()) return;

        //    if (this._allowedRanges == null)
        //    {
        //        this._allowedRanges = new Dictionary<int, int>();

        //        foreach (string strRange in RowsRange.Split(','))
        //        {
        //            if (strRange.Contains("-"))

        //                this._allowedRanges.Add(Convert.ToInt32(strRange.Split('-')[0]), Convert.ToInt32(strRange.Split('-')[1]));
        //            else
        //                this._allowedRanges.Add(Convert.ToInt32(strRange), Convert.ToInt32(strRange));
        //        }
        //    }
        //}
    }
}