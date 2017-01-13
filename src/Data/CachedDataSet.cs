using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public abstract class CachedDataSet : IDataSet
    {
        public string DataSetId { get; }
        private readonly Dictionary<string, IValuesObject> _objectsCache = new Dictionary<string, IValuesObject>();

        protected CachedDataSet(string dataSetId)
        {
            DataSetId = dataSetId;
        }

        public IValuesObject GetObjectByKey(string objectKey, Func<IValuesObject, string> evaluateKey)
        {
            if (!_objectsCache.Any())
            {
                LoadObjectsToCache(evaluateKey);
            }

            IValuesObject cachedObject;
            _objectsCache.TryGetValue(objectKey.ToUpper().Trim(), out cachedObject);

            return cachedObject;
        }

        private void LoadObjectsToCache(Func<IValuesObject, string> evaluateKey)
        {
            var targetObjects = this.ToList();
            targetObjects.ForEach(o => PutObjectToCache(o, evaluateKey));
        }

        public void PutObjectToCache(IValuesObject tObject, Func<IValuesObject, string> evaluateKey)
        {
            var tObjectKey = evaluateKey(tObject);
            if (tObjectKey.IsNotEmpty())
                _objectsCache[tObjectKey.ToUpper().Trim()] = tObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<IValuesObject> GetEnumerator();
        public abstract void Dispose();
        public abstract IValuesObject CreateObject();
    }
}