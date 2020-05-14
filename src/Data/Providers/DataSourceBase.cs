using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
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
    public abstract class DataSourceBase : IDataSource
    {
        public string Query { get; set; }

        public TransitionNode Key { get; set; }


        protected Dictionary<string, List<IValuesObject>> _cache;

        protected abstract IEnumerable<IValuesObject> GetDataInternal();

        public IEnumerable<IValuesObject> GetData()
        {
            LoadObjectsToCache();
            return _cache.Values.SelectMany(i => i);
        }

        public IEnumerable<IValuesObject> GetObjectsByKey(string key)
        {
            LoadObjectsToCache();

            string unifiedKey = UnifyKey(key);
            return _cache.ContainsKey(unifiedKey) ? _cache[unifiedKey] : null;
        }

        protected void LoadObjectsToCache()
        {
            if (_cache != null)
                return;

            var tracer = Migrator.Current.Tracer;
            tracer.TraceLine($"DataSource ({ this })");
            tracer.Indent();
            tracer.TraceLine($"Loading objects...");

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var targetObjects = GetDataInternal().ToList();
            stopwatch.Stop();

            tracer.TraceLine($"Loading {targetObjects.Count} objects completed in { stopwatch.Elapsed.TotalSeconds } sec");

            stopwatch.Reset();
            stopwatch.Start();

            targetObjects.ForEach(SetObjectKey);
            
            _cache = targetObjects
                .Where(i => i.Key.IsNotEmpty())
                .GroupBy(i => i.Key)
                .ToDictionary(i => i.Key, i => i.ToList());
            
            stopwatch.Stop();

            tracer.TraceLine($"Put {targetObjects.Count} objects to cache completed in { stopwatch.Elapsed.TotalSeconds } sec");
            tracer.IndentBack();

        }

        protected void PutObjectToCache(IValuesObject tObject)
        {
            if (tObject.Key.IsEmpty())
                return;

            if (!_cache.ContainsKey(tObject.Key))
                _cache.Add(tObject.Key, new List<IValuesObject>());

            if (_cache[tObject.Key].Contains(tObject))
                return;
            
            _cache[tObject.Key].Add(tObject);
        }
        
        private void SetObjectKey(IValuesObject valuesObject)
        {
            var ctx = new ValueTransitContext(valuesObject, null, valuesObject);

            var result = Key.Transit(ctx);
            valuesObject.Key = result.Value != null ? UnifyKey(result.Value.ToString()) : null; 
        }

        protected static string UnifyKey(string key)
        {
             return key.Trim().ToUpper();
        }

        public override string ToString()
        {
            return $"Query: { Query }, Key: { Key }";
        }
    }
}