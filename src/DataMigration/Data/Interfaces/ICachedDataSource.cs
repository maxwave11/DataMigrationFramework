using System;
using System.Collections.Generic;

namespace DataMigration.Data.Interfaces;

public interface ICachedDataSource<TSource>: IDataSource<TSource>
{
    TSource GetNewObject(string key)
    {
        throw new NotImplementedException();
    }
    IEnumerable<TSource> GetObjectsByKey(string key);
}