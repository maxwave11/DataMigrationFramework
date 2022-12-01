using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataMigration.Data.Interfaces;
using DataMigration.Utils;

namespace DataMigration.Data;

public class Cache<T> where T : IDataObject
{
    private Dictionary<string, List<T>> _cache;
    private readonly IDataSource<T> _dataSource;

    public Cache(IDataSource<T> dataSource)
    {
        _dataSource = dataSource;
    }

    public IEnumerable<T> GetObjectsByKey(string key)
    {
        LoadObjectsToCache();

        string normalizedKey = NormalizeKey(key);
        return _cache.ContainsKey(normalizedKey) ? _cache[normalizedKey] : Enumerable.Empty<T>();
    }

    private void LoadObjectsToCache()
    {
        if (_cache != null)
            return;

        var stopwatch = new Stopwatch();
        stopwatch.Start();


        var allItems = GetDataWithKeys().ToList();

        _cache = allItems
            .GroupBy(i => NormalizeKey(i.Key))
            .ToDictionary(i => i.Key, i => i.ToList());

        stopwatch.Stop();
    }

    public void Add(T dataObject)
    {
        if (dataObject.Key.IsEmpty())
            return;

        var key = NormalizeKey(dataObject.Key);
            
        if (!_cache.ContainsKey(key))
            _cache.Add(key, new List<T>());

        if (_cache[key].Contains(dataObject))
            return;
        
        _cache[key].Add(dataObject);
    }

    public void Remove(string key)
    {
        var normalizedKey = NormalizeKey(key);
        _cache.Remove(normalizedKey);
    }

    private IEnumerable<T> GetDataWithKeys()
    {
        foreach (var dataObject in _dataSource.GetData())
        {
            var key = _dataSource.GetObjectKey(dataObject);

            if (key.IsEmpty())
                continue;

            dataObject.Key  = key;
            yield return dataObject;
        }
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToUpper();
    }
}