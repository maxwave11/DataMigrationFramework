using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data.DataSources
{
    public abstract class DataSourceBase : IDataSource, ICachedDataSource
    {
        public string Query { get; set; }

        public CommandBase Key { get; set; }
        
        public ExpressionCommand<bool> Filter { get; set; }

        
        /// <summary>
        /// Some addition commands to preare (unify) data when using many data sources with different structure
        /// For example in case when there is fiew files with same data but with different headers
        /// </summary>
        public CommandSet<CommandBase> PrepareData { get; set; }

        protected Dictionary<string, List<IValuesObject>> _cache;

        protected abstract IEnumerable<IValuesObject> GetDataInternal();

        public IEnumerable<IValuesObject> GetData()
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

        public IEnumerable<IValuesObject> GetCachedData()
        {
            LoadObjectsToCache();
            return _cache.SelectMany(i => i.Value);
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
            
            _cache = GetData()
                .GroupBy(i => i.Key)
                .ToDictionary(i => i.Key, i => i.ToList());
            
            stopwatch.Stop();
            tracer.TraceLine($"Loading {_cache.Values.Sum(i=>i.Count)} objects completed in { stopwatch.Elapsed.TotalSeconds } sec");
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
        
        protected static string UnifyKey(string key)
        {
             return key.Trim().ToUpper();
        }

        public override string ToString()
        {
            return $"Query: { Query }, Key: { Key.GetParametersInfo() }";
        }
    }
}