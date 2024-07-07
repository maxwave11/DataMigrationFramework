using System.Threading;
using DataMigrationCore.Enums;

namespace DataMigrationCore.Pipeline.Operations;

internal class FlowOperation : IOperation
{
    private readonly PipelineFlowControl _flow;
    private readonly string _message;
    public IOperation NextOperation { get; set; }

    public FlowOperation(PipelineFlowControl flow, string message)
    {
        _flow = flow;
        _message = message;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        runContext.FlowControl = _flow;
    }

    public override string ToString()
    {
        return "FLOW";
    }
}
