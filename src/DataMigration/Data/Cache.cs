using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataMigration.Data.Interfaces;
using DataMigration.Utils;

namespace DataMigration.Data;

public class Cache<T>
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
            .GroupBy(dataWithKey => NormalizeKey(dataWithKey.key))
            .ToDictionary(group => group.Key, group => group.Select(dataWithKey => dataWithKey.dataObject).ToList());

        stopwatch.Stop();
    }

    public void Add(string key, T dataObject)
    {
        if (key.IsEmpty())
            throw new InvalidOperationException("Add object with empty key is not allowed");

        var normalizedKey = NormalizeKey(key);
            
        if (!_cache.ContainsKey(normalizedKey))
            _cache.Add(normalizedKey, new List<T>());

        if (_cache[normalizedKey].Contains(dataObject))
            return;
        
        _cache[normalizedKey].Add(dataObject);
    }

    public void Remove(string key)
    {
        var normalizedKey = NormalizeKey(key);
        _cache.Remove(normalizedKey);
    }

    private IEnumerable<(string key, T dataObject)> GetDataWithKeys()
    {
        foreach (var dataObject in _dataSource.GetData())
        {
            var key = _dataSource.GetObjectKey(dataObject);

            if (key.IsEmpty())
                continue;
            
            yield return (key, dataObject);
        }
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToUpper();
    }
}