using System;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Operations;

public class IfOperation<TContext, TOutput> : IOperation<TContext, TOutput> where TContext: IPipeContext
{
    public IPipeContext Context { get; }
    public IOperation NextOperation { get; set; }
    
    private readonly Predicate<TContext> _predicate;
    private readonly PipelineFlowControl _ifFalseFlow = PipelineFlowControl.SkipValue;
    private readonly string _ifFalseMessage;


    public IfOperation(Predicate<TContext> predicate, IPipeContext context)
    {
        _predicate = predicate;
        Context = context;
    }
    
    public IfOperation(Predicate<TContext> predicate, IPipeContext context, PipelineFlowControl ifFalseFlow, string ifFalseMessage)
    {
        _predicate = predicate;
        _ifFalseFlow = ifFalseFlow;
        _ifFalseMessage = ifFalseMessage;
        Context = context;
    }
    
    public object Execute()
    {
        if (!_predicate((TContext)Context))
        {
            Context.FlowControl = _ifFalseFlow;
            Context.Message = _ifFalseMessage;
        }
        else
        {
            Context.FlowControl = PipelineFlowControl.Continue;
        }

        return Context.GetValue();
    }

    public override string ToString()
    {
        return "IF";
    }
}