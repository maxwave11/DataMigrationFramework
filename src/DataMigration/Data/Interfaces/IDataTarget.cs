using System.Collections.Generic;

namespace DataMigration.Data.Interfaces
{
    public interface IDataTarget<TTarget>: ICachedDataSource<TTarget>
    {
        void SaveObjects(IEnumerable<TTarget> objectsToSave);
        void InvalidateObject(string key);

        public bool IsObjectNew(string key);
    }
}