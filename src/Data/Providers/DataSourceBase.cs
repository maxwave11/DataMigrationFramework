using System;
using System.Collections.Generic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;

namespace XQ.DataMigration.Data
{
    public abstract class DataSourceBase : IDataSource 
    {
        public string Query { get; set; }

        public TransitValueCommand Key { get; set; }

        /// <summary>
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        public string RowsRange { get; set; }

        //private Dictionary<int, int> _allowedRanges;

        protected abstract IEnumerable<IValuesObject> GetDataInternal();
        public IEnumerable<IValuesObject> GetData() {

            var srcDataSet = GetDataInternal();
            return srcDataSet;

            //var rowNumber = 0;


            //foreach (var sourceObject in srcDataSet)
            //{
            //    rowNumber++;

            //    if (!IsRowIndexInRange(rowNumber))
            //        continue;

            //    sourceObject.SetValue("RowNumber", rowNumber);
            //}
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