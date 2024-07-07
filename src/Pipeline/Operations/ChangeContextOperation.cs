using System;
using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class ChangeContextOperationConfig<TNewContext> : IOperationConfig
{
    private readonly Func<object, object, TNewContext> _func;
    private readonly string _expressionView;

    internal ChangeContextOperationConfig(Func<object, object, TNewContext> func, string expressionView)
    {
        _func = func;
        _expressionView = expressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new ChangeContextOperation<TNewContext>(_func, _expressionView);
    }
}


internal class ChangeContextOperation<TNewContext> : IOperation
{
    private readonly Func<object, object, TNewContext> _func;
    private readonly string _expression;

    public ChangeContextOperation(Func<object, object, TNewContext> func, string expression)
    {
        _func = func;
        _expression = expression;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        runContext.SetCustomContext(_func(runContext.Context, runContext.Value));
    }

    public override string ToString()
    {
        return "WITH CONTEXT " + _expression;
    }
}
