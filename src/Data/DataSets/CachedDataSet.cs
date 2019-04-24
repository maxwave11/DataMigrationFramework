using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public abstract class CachedDataSet: IEnumerable<IValuesObject>
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
                LoadObjectsToCache(evaluateKey);

            IValuesObject cachedObject;
            _objectsCache.TryGetValue(objectKey.ToUpper().Trim(), out cachedObject);

            return cachedObject;
        }

        private void LoadObjectsToCache(Func<IValuesObject, string> evaluateKey)
        {
            Migrator.Current.Tracer.TraceLine($"Loading objects ({ DataSetId })...");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var targetObjects = this.ToList();
            stopwatch.Stop();

            Migrator.Current.Tracer.TraceLine($"Loading completed in { stopwatch.Elapsed.Seconds } sec");
            
            targetObjects.ForEach(o => PutObjectToCache(o, evaluateKey));
        }

        public void PutObjectToCache(IValuesObject tObject, Func<IValuesObject, string> evaluateKey)
        {
            PutObjectToCache(tObject, evaluateKey(tObject));
        }

        public void PutObjectToCache(IValuesObject tObject, string objectkey)
        {
            if (objectkey.IsEmpty())
                return;

            _objectsCache[objectkey.ToUpper().Trim()] = tObject;
        }

        public void RemoveObjectFromCache(string objectkey)
        {
            if (objectkey.IsEmpty())
                return;

            _objectsCache.Remove(objectkey.ToUpper().Trim());
        }

        public abstract IEnumerator<IValuesObject> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}