using System;
using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    /// <summary>
    /// Common interface for accessing to particular objects (entries, rows) of some data provder
    /// </summary>
    public interface IDataSet : IEnumerable<IValuesObject>
    {
        string DataSetId { get; }

        /// <summary>
        /// Extract object from current DataSet by unique object key. Object key defined always defined
        /// in mapping configuration file in KeyDefinition element 
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="evaluateKey"></param>
        /// <returns></returns>
        IValuesObject GetObjectByKey(string objectKey, Func<IValuesObject, string> evaluateKey);
    }
}