using System;
using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    public interface ITargetProvider : IDataProvider
    {
        void SaveObjects(ICollection<IValuesObject> objects);
        IValuesObject CreateObject(string objectType, string key);
        void RemoveObjectFromCache(string objectType, string key);
        IValuesObject GetObjectByKey(string objType, string objectKey, Func<IValuesObject, string> evaluateKey, string queryToTarget);
    }
}