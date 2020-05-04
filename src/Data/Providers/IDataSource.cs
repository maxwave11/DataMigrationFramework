using System;
using System.Collections.Generic;
using XQ.DataMigration.MapConfig;

namespace XQ.DataMigration.Data
{
    /// <summary>
    /// Common interface for any data provider which provides access to 
    /// particular DataSet of this provider
    /// </summary>
    public interface IDataSource { 
        IEnumerable<IValuesObject> GetData();
        //KeyTransition Key { get; }
    }
}