using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataMigrationCore.Data.Interfaces;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Data;

internal class PipelineSource<TSource> : IPipelineSource
{
    private readonly IEnumerable<TSource> _underlyingSource;
    private readonly IMigrationLogger _logger;
    private readonly Func<TSource, object> _contextSelector;

    internal PipelineSource(IEnumerable<TSource> underlyingSource, IMigrationLogger logger,
        Func<TSource, object> contextSelector = null)
    {
        _underlyingSource = underlyingSource;
        _logger = logger;
        _contextSelector = contextSelector;
    }

    private IEnumerable GetContextItems()
    {
        if (_contextSelector != null)
            return _underlyingSource.Select(item => _contextSelector(item));

        return _underlyingSource;
    }

    public IEnumerator GetEnumerator()
    {
        return GetContextItems().GetEnumerator();
    }

    public int Count()
    {
        switch (_underlyingSource)
        {
            case IQueryable<TSource> queryableSource:
                return queryableSource.Count();
            case PagedSource<TSource> pagedSource:
                return pagedSource.Count();
            case ICollection<TSource> collectionSource:
                return collectionSource.Count();
            default:
            {
                _logger.LogWarning(
                    @$"Using default method of counting DataSource {_underlyingSource.GetType()} objects.
It may significantly affect on performance and memory consumption. Try to avoid it by using custom counting objects method");
                return 0;
            }
        }
    }
}
