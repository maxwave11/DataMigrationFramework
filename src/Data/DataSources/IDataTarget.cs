using System.Collections.Generic;

namespace XQ.DataMigration.Data.DataSources
{
    public interface IDataTarget: IDataSource
    {
        IValuesObject GetObjectByKeyOrCreate(string key);
        void SaveObjects(IEnumerable<IValuesObject> objects);
        void InvalidateObject(IValuesObject valuesObject);
    }
}