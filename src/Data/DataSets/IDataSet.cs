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
        /// Extract an object from current DataSet by unique object key. Object's key always must be defined
        /// in mapping configuration file in <c>KeyDefinition</c> 
        IValuesObject GetObjectByKey(string objectKey, Func<IValuesObject, string> evaluateKey);

        /// <summary>
        /// Extract a first (or null) object from current DataSet by some expression. If result value of expression evaluation on any oject
        /// is equal to  <paramref name="valueToFind"/> then this object will be returned.
        IValuesObject GetObjectByExpression(string valueToFind, Func<IValuesObject, string> evaluateExpression, Func<IValuesObject, string> evaluateKey);
    }
}