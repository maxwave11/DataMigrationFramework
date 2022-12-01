using DataMigration.Enums;

namespace DataMigration.Pipeline.Operations;

public class FlowOperation<TContext, TOutput> : IOperation<TContext, TOutput> where TContext: IPipeContext
{
    private readonly PipelineFlowControl _flow;
    private readonly string _message;
    public IPipeContext Context { get; }
    public IOperation NextOperation { get; set; }
    
    public FlowOperation(PipelineFlowControl flow, TContext context, string message)
    {
        _flow = flow;
        _message = message;
        Context = context;
    }
    
    public object Execute()
    {
        Context.FlowControl = _flow;
        Context.Message = _message;
        return Context.GetValue();
    }

    public override string ToString()
    {
        return "FLOW";
    }
}