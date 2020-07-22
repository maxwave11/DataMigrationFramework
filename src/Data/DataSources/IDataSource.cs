using System.Collections.Generic;

namespace XQ.DataMigration.Data.DataSources
{
    /// <summary>
    /// Common interface for any data provider which provides access to 
    /// particular DataSet of this provider
    /// </summary>
    public interface IDataSource 
    { 
        IEnumerable<IDataObject> GetData();
    }

    public interface ICachedDataSource
    {
        IEnumerable<IDataObject> GetCachedData();
        IEnumerable<IDataObject> GetObjectsByKey(string key);
    }
}