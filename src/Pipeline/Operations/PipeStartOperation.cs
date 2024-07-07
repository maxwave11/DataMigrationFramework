using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class PipeStartOperationConfig : IOperationConfig
{
    private readonly string _pipeName;

    public PipeStartOperationConfig(string pipeName)
    {
        _pipeName = pipeName;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new PipeStartOperation(_pipeName);
    }
}

internal class PipeStartOperation : IOperation
{
    private readonly string _pipeName;

    public PipeStartOperation(string pipeName)
    {
        _pipeName = pipeName;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        runContext.RestoreOriginalContext();
    }

    public override string ToString()
    {
        return $"PIPE '{_pipeName}'";
    }
}
