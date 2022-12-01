using System.Collections.Generic;

namespace DataMigration.Data.Interfaces
{
    /// <summary>
    /// Common interface for any data source which returns data from some sources
    /// like database, excel, csv, etc...
    /// </summary>
    public interface IDataSource<TSource>
    {
        string GetObjectKey(TSource dataObject);
        IEnumerable<TSource> GetData();
    }
}