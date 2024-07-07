using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataMigrationCore.Logging;
using DataMigrationCore.Utils;

namespace DataMigrationCore.Data;

internal class LookupDataSource<T>
{
    private readonly IEnumerable<T> _dataSource;
    private readonly Func<T, string> _keySelector;
    private readonly bool _enableCache;
    private readonly StringComparison _keyComparisonType;
    private readonly IMigrationLogger _logger;
    private Dictionary<string, List<T>> _cache;

    public LookupDataSource(
        IEnumerable<T> dataSource,
        Func<T, string> keySelector,
        IMigrationLogger logger,
        bool enableCache = true,
        StringComparison keyComparisonType = StringComparison.InvariantCultureIgnoreCase)
    {
        _dataSource = dataSource;
        _keySelector = keySelector;
        _enableCache = enableCache;
        _keyComparisonType = keyComparisonType;
        _logger = logger;
    }

    public IEnumerable<T> GetObjectsByKey(string key)
    {
        if (_enableCache)
            return GetObjectsByKeyFromCache(key);

        return GetObjectsByKeyFromDataSource(key);
    }

    private IEnumerable<T> GetObjectsByKeyFromCache(string key)
    {
        if (_cache == null)
            LoadObjectsToCache();

        return _cache.ContainsKey(key) ? _cache[key] : Enumerable.Empty<T>();
    }

    private IEnumerable<T> GetObjectsByKeyFromDataSource(string key)
    {
        return _dataSource
            .Where(dataObject => string.Equals(_keySelector(dataObject), key, _keyComparisonType))
            .ToList();
    }

    private void LoadObjectsToCache()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _logger.LogInformation($"Loading objects of type {typeof(T)} to cache...");

        var dataWithKeys = GetDataWithKeys().ToList();

        stopwatch.Stop();

        _logger.LogInformation(
            $"Loaded {dataWithKeys.Count} objects of type {typeof(T)} in {stopwatch.ElapsedMilliseconds} ms");

        _cache = dataWithKeys
            .GroupBy(dataWithKey => dataWithKey.key)
            .ToDictionary(
                keySelector: group => group.Key,
                elementSelector: group => group.Select(dataWithKey => dataWithKey.dataObject).ToList(),
                comparer: StringComparer.FromComparison(_keyComparisonType));
    }

    public void AddToCache(string key, T dataObject)
    {
        if (key.IsEmpty())
            throw new InvalidOperationException("Add object with empty key is not allowed");


        if (!_cache.ContainsKey(key))
            _cache.Add(key, new List<T>());

        if (_cache[key].Contains(dataObject))
            return;

        _cache[key].Add(dataObject);
    }

    private IEnumerable<(string key, T dataObject)> GetDataWithKeys()
    {
        foreach (var dataObject in _dataSource)
        {
            string key;
            try
            {
                key = _keySelector(dataObject);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during key evaluation", ex);
            }

            if (key.IsEmpty())
                continue;

            yield return (key, dataObject);
        }
    }
}
