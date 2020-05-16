using System.Collections.Generic;

namespace XQ.DataMigration.Data.DataSources
{
    public interface ITargetSource: IDataSource
    {
        IValuesObject GetObjectByKeyOrCreate(string key);
        void SaveObjects(IEnumerable<IValuesObject> objects);
        void InvalidateObject(IValuesObject valuesObject);
    }
}