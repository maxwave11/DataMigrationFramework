using System;
using System.Collections.Generic;
using XQ.DataMigration.Mapping.TransitionNodes;

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
        void Initialize();
        IDataSet GetDataSet(string providerQuery);
    }
}