using System.Collections.Generic;

namespace DataMigration.Data.Interfaces
{
    /// <summary>
    /// Common interface for any data source which returns data from some sources
    /// like database, excel, csv, etc...
    /// </summary>
    public interface IDataSource 
    {
        IEnumerable<IDataObject> GetData();
        string GetObjectKey(IDataObject dataObject);
    }
    
    public interface IDataSource<T>: IDataSource where T: IDataObject
    {
        new string GetObjectKey(T dataObject);
        new IEnumerable<T> GetData();
        
        IEnumerable<IDataObject> IDataSource.GetData() => (IEnumerable<IDataObject>)GetData();
        string IDataSource.GetObjectKey(IDataObject dataObject) => GetObjectKey((T)dataObject);
    }
}