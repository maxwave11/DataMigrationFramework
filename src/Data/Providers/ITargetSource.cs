using System;
using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    public interface ITargetSource: IDataSource
    {
        IValuesObject GetObjectByKeyOrCreate(string key);
        void SaveObjects(IEnumerable<IValuesObject> objects);
        void InvalidateObject(IValuesObject valuesObject);
    }
}