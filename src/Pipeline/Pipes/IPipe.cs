using System.Threading;

namespace DataMigrationCore.Pipeline.Pipes;

public interface IPipe
{
    void Execute(PipelineRunContext runContext, CancellationToken cancellationToken);
}
