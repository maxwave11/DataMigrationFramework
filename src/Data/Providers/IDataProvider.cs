using System;
using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    /// <summary>
    /// Common interface for any data provider which provides access to 
    /// particular DataSet of this provider
    /// </summary>
    public interface IDataProvider { 
        string Name { get; set; }
        bool IsDefault { get; set; }
        string Query { get; set; }
        IEnumerable<IValuesObject> GetDataSet(string providerQuery);
    }
}