using System.Collections;

namespace DataMigrationCore.Data.Interfaces;

internal interface IPipelineSource : IEnumerable
{
    int Count();
}
