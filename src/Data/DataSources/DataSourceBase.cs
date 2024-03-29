using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;
using DataMigration.Utils;

namespace DataMigration.Data.DataSources
{
    /// <summary>
    /// Base data source functionality. Use it as base for your custom data providers.
    /// </summary>
    public abstract class DataSourceBase : IDataSource, ICachedDataSource
    {
        public object Query { get; set; }

        /// <summary>
        /// Expression returns unique key for each entry from data source
        /// </summary>
        public CommandBase Key { get; set; }
        
        public ExpressionCommand<bool> Filter { get; set; }

        
        /// <summary>
        /// Some addition commands to prepare (unify) data when using many data sources with different structure
        /// For example in case when there is few files with same data but with different headers
        /// </summary>
        public CommandSet<CommandBase> PrepareData { get; set; }

        protected Dictionary<string, List<IDataObject>> _cache;

        protected abstract IEnumerable<IDataObject> GetDataInternal();

        public IEnumerable<IDataObject> GetData()
        {
            Migrator.Current.Tracer.TraceLine($"DataSource ({ this }) - Get data...");

            uint rowCounter = 0;

            foreach (var valuesObject in GetDataInternal())
            {
                rowCounter++;
                var ctx = new ValueTransitContext(valuesObject, valuesObject);

                if (Filter != null && ctx.Execute(Filter) == false)
                    continue;
                
                ctx.Execute(Key);
                var strKey = ctx.TransitValue?.ToString();
                
                if (strKey.IsEmpty())
                    continue;
                
                valuesObject.Key = UnifyKey(strKey); 
                valuesObject.RowNumber = rowCounter;
                if (PrepareData!=null) ctx.Execute(PrepareData);
                yield return valuesObject;
            }
        }

        public IEnumerable<IDataObject> GetCachedData()
        {
            LoadObjectsToCache();
            return _cache.SelectMany(i => i.Value);
        }

        public IEnumerable<IDataObject> GetObjectsByKey(string key)
        {
            LoadObjectsToCache();

            string unifiedKey = UnifyKey(key);
            return _cache.ContainsKey(unifiedKey) ? _cache[unifiedKey] : Enumerable.Empty<IDataObject>();
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
            
            _cache = GetData()
                .GroupBy(i => i.Key)
                .ToDictionary(i => i.Key, i => i.ToList());
            
            stopwatch.Stop();
            tracer.TraceLine($"Loading {_cache.Values.Sum(i=>i.Count)} objects completed in { stopwatch.Elapsed.TotalSeconds } sec");
            tracer.IndentBack();
        }

        protected void PutObjectToCache(IDataObject tObject)
        {
            if (tObject.Key.IsEmpty())
                return;

            if (!_cache.ContainsKey(tObject.Key))
                _cache.Add(tObject.Key, new List<IDataObject>());

            if (_cache[tObject.Key].Contains(tObject))
                return;
            
            _cache[tObject.Key].Add(tObject);
        }

        private string _actualQuery;
        protected string ActualQuery
        {
            get
            {
                if (_actualQuery != null) return _actualQuery;
                
                switch (Query)
                {
                    case string query:
                        _actualQuery = query;
                        break;
                    case CommandBase command:
                        var ctx = new ValueTransitContext(null, null);
                        ctx.Execute(command);
                        _actualQuery = ctx.TransitValue?.ToString();
                        break;
                    default:
                        throw new NotSupportedException("Only string and Commands are supported in Query");
                }

                return _actualQuery;
            }
        }
        
        protected static string UnifyKey(string key)
        {
             return key.Trim().ToUpper();
        }

        public override string ToString()
        {
            return $"Query: { ActualQuery }, Key: { Key.GetParametersInfo() }";
        }
    }
}