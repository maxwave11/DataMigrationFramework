using System;
using System.Threading;
using DataMigrationCore.Enums;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class IfOperationConfig : IOperationConfig
{
    private readonly Func<object, object, bool> _predicate;
    private readonly PipelineFlowControl _ifFalseFlowControl;
    private readonly string _expressionView;

    internal IfOperationConfig(Func<object, object, bool> predicate,
        PipelineFlowControl ifFalseFlowControl, string expressionView)
    {
        _predicate = predicate;
        _ifFalseFlowControl = ifFalseFlowControl;
        _expressionView = expressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new IfOperation(_predicate, _ifFalseFlowControl, _expressionView);
    }
}

internal class IfOperation : IOperation
{
    private readonly Func<object, object, bool> _predicate;
    private readonly PipelineFlowControl _ifFalseFlow = PipelineFlowControl.SkipPipe;
    private readonly string _expressionView;

    public IfOperation(Func<object, object, bool> predicate, PipelineFlowControl ifFalseFlowControl, string expressionView = "")
    {
        _ifFalseFlow = ifFalseFlowControl;
        _expressionView = expressionView;
        _predicate = predicate;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        if (!_predicate(runContext.Context, runContext.Value))
        {
            runContext.FlowControl = _ifFalseFlow;
        }
        else
        {
            runContext.FlowControl = PipelineFlowControl.Continue;
        }
    }

    public override string ToString()
    {
        return "IF " + _expressionView;
    }
}
