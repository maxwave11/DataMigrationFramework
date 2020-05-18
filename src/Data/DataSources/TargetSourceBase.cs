using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Data.DataSources
{
    public abstract class TargetSourceBase : DataSourceBase, ITargetSource
    {
        protected abstract IValuesObject CreateObject(string key);
        public abstract void SaveObjects(IEnumerable<IValuesObject> objects);

        public ObjectTransitMode TransitMode { get; set; }

        public IValuesObject GetObjectByKeyOrCreate(string key)
        {
            LoadObjectsToCache();

            string unifiedKey = UnifyKey(key);

            var targetObject = _cache.ContainsKey(unifiedKey) ? _cache[unifiedKey].SingleOrDefault() : null;

            switch (TransitMode)
            {
                case ObjectTransitMode.OnlyExistedObjects:
                    return targetObject;
                case ObjectTransitMode.OnlyNewObjects when targetObject != null:
                    return null;
            }

            if (targetObject != null)
                return targetObject;

            targetObject = CreateObject(key);

            PutObjectToCache(targetObject);

            return targetObject;
        }

        public void InvalidateObject(IValuesObject valuesObject)
        {
            _cache.Remove(valuesObject.Key);
        }
    }
}