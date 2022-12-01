using System;
using System.Collections.Generic;

namespace DataMigration.Data.Interfaces;

public interface ICachedDataSource<T>: IDataSource<T> where T : IDataObject
{
    T GetNewObject(string key)
    {
        throw new NotImplementedException();
    }
    IEnumerable<T> GetObjectsByKey(string key);
}