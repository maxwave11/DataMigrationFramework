using System.Collections.Generic;

namespace DataMigration.Data.DataSources
{
    /// <summary>
    /// Common interface for any data source which returns data from some sources
    /// like database, excel, csv, etc...
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