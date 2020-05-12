using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public abstract class DataSourceBase : IDataSource
    {
        public string Query { get; set; }

        public ConcatReadTransition Key { get; set; }

        
        private Dictionary<string, List<IValuesObject>> _cache;

        /// <summary>
        /// Set this value if you want to transit concrete range of DataSet objects from source system
        /// Example 1: 2-10
        /// Example 2: 2-10, 14-50
        /// </summary>
        public string RowsRange { get; set; }

        //private Dictionary<int, int> _allowedRanges;

        protected abstract IEnumerable<IValuesObject> GetDataInternal();

        public IEnumerable<IValuesObject> GetData()
        {
            if (_cache == null)
                LoadObjectsToCache();

            return _cache.Values.SelectMany(i => i);
        }

        public IEnumerable<IValuesObject> GetObjectsByKey(string key)
        {
            if (_cache == null)
                LoadObjectsToCache();

            string unifiedKey = UnifyKey(key);
            return _cache.ContainsKey(unifiedKey) ? _cache[unifiedKey] : null;
        }

        private void LoadObjectsToCache()
        {
            Migrator.Current.Tracer.TraceLine($"DataSource ({ this }) - Loading objects...");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var targetObjects = GetDataInternal().ToList();
            stopwatch.Stop();

            Migrator.Current.Tracer.TraceLine($"Loading {targetObjects.Count} objects completed in { stopwatch.Elapsed.TotalSeconds } sec");

            Migrator.Current.Tracer.TraceLine($"DataSource ({ this }) - Put {targetObjects.Count} objects to cache...");

            stopwatch.Reset();
            stopwatch.Start();

            targetObjects.ForEach(SetObjectKey);
            
            _cache = targetObjects
                .Where(i => i.Key.IsNotEmpty())
                .GroupBy(i => i.Key)
                .ToDictionary(i => i.Key, i => i.ToList());
            
            stopwatch.Stop();

            Migrator.Current.Tracer.TraceLine($"DataSource ({ this }) - Put {targetObjects.Count} objects to cache completed in { stopwatch.Elapsed.TotalSeconds } sec");
        }

        protected void PutObjectToCache(IValuesObject tObject)
        {
            if (tObject.IsEmpty())
                return;

            if (!_cache.ContainsKey(tObject.Key))
                _cache.Add(tObject.Key, new List<IValuesObject>());

            if (_cache[tObject.Key].Contains(tObject))
                return;
            
            _cache[tObject.Key].Add(tObject);
        }
        
        private void SetObjectKey(IValuesObject valuesObject)
        {
            var result =  Key.Transit(new ValueTransitContext(valuesObject,null, valuesObject));
            valuesObject.Key = UnifyKey(result.Value?.ToString()); 
        }

        private static string UnifyKey(string key)
        {
            return key.Trim().ToUpper();
        }

        public override string ToString()
        {
            return $"Query: { Query }, Key: { Key }";
        }

        //private bool IsRowIndexInRange(int rowIndex)
        //{
        //    if (RowsRange.IsEmpty()) return true;

        //    return _allowedRanges.Any(i => i.Key <= rowIndex && rowIndex <= i.Value);
        //}

        //private void ParseRowsRange()
        //{
        //    if (RowsRange.IsEmpty()) return;

        //    if (this._allowedRanges == null)
        //    {
        //        this._allowedRanges = new Dictionary<int, int>();

        //        foreach (string strRange in RowsRange.Split(','))
        //        {
        //            if (strRange.Contains("-"))

        //                this._allowedRanges.Add(Convert.ToInt32(strRange.Split('-')[0]), Convert.ToInt32(strRange.Split('-')[1]));
        //            else
        //                this._allowedRanges.Add(Convert.ToInt32(strRange), Convert.ToInt32(strRange));
        //        }
        //    }
        //}
    }
}