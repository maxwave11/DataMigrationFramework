using System;
using System.Collections;
using System.Collections.Generic;

namespace DataMigrationCore.Data;

public class PagedSource<T> : IEnumerable<T>
{
    public Func<Page, IEnumerable<T>> Source { get; init; }
    public int PageSize { get; init; }
    public Func<int> ObjectsCount { get; init; }

    private IEnumerable<T> GetObjects()
    {
        if (PageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(PageSize));

        var pageObjectsHandled = 0;
        var pageIndex = 0;

        do
        {
            var page = new Page(PageSize, pageIndex * PageSize);
            pageObjectsHandled = 0;

            foreach (var sourceObject in Source(page))
            {
                pageObjectsHandled++;
                yield return sourceObject;
            }

            pageIndex++;

        } while (PageSize == pageObjectsHandled);
    }

    public int Count()
    {
        return ObjectsCount?.Invoke() ?? -1;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return GetObjects().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
