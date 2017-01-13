using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    public interface ITargetProvider : IDataProvider
    {
        void Initialize(string settings);
        void SaveObjects(ICollection<IValuesObject> objects);
        IValuesObject CreateObject(string dataSetId);
        new CachedDataSet GetDataSet(string dataSetId);
    }
}