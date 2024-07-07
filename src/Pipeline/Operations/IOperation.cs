using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

public interface IOperation
{
    void Execute(PipelineRunContext runContext, CancellationToken cancellationToken);
}

public interface IOperationConfig
{
    IOperation CreateOperation(IMigrationLogger logger);
}
