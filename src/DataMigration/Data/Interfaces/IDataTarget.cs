using System.Collections.Generic;

namespace DataMigration.Data.Interfaces
{
    public interface IDataTarget<TTarget>: ICachedDataSource<TTarget> where TTarget : IDataObject
    {
        void SaveObjects(IEnumerable<TTarget> objects);
        void InvalidateObject(TTarget dataObject);
    }
}