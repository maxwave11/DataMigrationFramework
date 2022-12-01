using System.Collections.Generic;
using System.Linq;
using DataMigration.Enums;

namespace DataMigration.Data.DataSources
{
    // public abstract class DataTargetBase<T> : DataSourceBase<T>, IDataTarget where T : IDataObject
    // {
    //     public abstract void SaveObjects(IEnumerable<IDataObject> objects);
    //
    //     public ObjectTransitMode TransitMode { get; set; }
    //
    //     public IDataObject GetObjectByKeyOrCreate(string key)
    //     {
    //         LoadObjectsToCache();
    //
    //         string unifiedKey = NormalizeKey(key);
    //
    //         var targetObject = _cache.ContainsKey(unifiedKey) ? _cache[unifiedKey].SingleOrDefault() : null;
    //
    //         switch (TransitMode)
    //         {
    //             case ObjectTransitMode.OnlyExistedObjects:
    //                 return targetObject;
    //             case ObjectTransitMode.OnlyNewObjects when targetObject != null:
    //                 return null;
    //         }
    //
    //         if (targetObject != null)
    //             return targetObject;
    //
    //         targetObject = GetNewObject(key);
    //
    //         PutObjectToCache(targetObject);
    //
    //         return targetObject;
    //     }
    //
    //     public void InvalidateObject(IDataObject dataObject)
    //     {
    //         _cache.Remove(NormalizeKey(dataObject.Key));
    //     }
    // }
}