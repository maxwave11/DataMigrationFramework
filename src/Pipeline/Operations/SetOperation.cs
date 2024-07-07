using System;
using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class SetOperationConfig : IOperationConfig
{
    private readonly Action<object, object> _action;
    private readonly string _expressionView;

    internal SetOperationConfig(Action<object, object> action, string expressionView)
    {
        _action = action;
        _expressionView = expressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new SetOperation(_action, _expressionView);
    }
}

internal class SetOperation : IOperation
{
    private readonly Action<object, object> _action;
    private readonly string _expressionView;

    public SetOperation(Action<object, object> action, string expressionView = "")
    {
        _action = action;
        _expressionView = expressionView;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        _action(runContext.Context, runContext.Value);
    }

    public override string ToString()
    {
        return "SET " + _expressionView;
    }
}
