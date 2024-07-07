using System;
using System.Collections.Generic;

namespace DataMigrationCore.Data;

public record PipelineResults(IReadOnlyCollection<object> ObjectsToAdd, IReadOnlyCollection<object> ObjectsToUpdate, TimeSpan ElapsedTime)
{
    public PipelineResults() : this(Array.Empty<object>(), Array.Empty<object>(), TimeSpan.Zero)
    {

    }
}
