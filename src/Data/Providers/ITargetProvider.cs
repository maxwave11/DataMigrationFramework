using System;
using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    public interface ITargetProvider : IDataSource
    {
        void SaveObjects(ICollection<IValuesObject> objects);
        IValuesObject CreateObject(string key);
        void RemoveObjectFromCache(string key);
        IValuesObject GetObjectByKey(string objectKey);
    }
}