using System;
using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    public interface ITargetProvider
    {
        IValuesObject CreateObject(string key);
        
        void SaveObjects(IEnumerable<IValuesObject> objects);
    }
}