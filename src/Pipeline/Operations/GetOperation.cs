using System;
using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class GetOperationConfig : IOperationConfig
{
    private readonly Func<object, object, object> _func;
    private readonly string _expressionView;

    internal GetOperationConfig(Func<object, object, object> func, string expressionView)
    {
        _func = func;
        _expressionView = expressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new GetOperation(_func, _expressionView);
    }
}

internal class GetOperation : IOperation
{
    private readonly Func<object, object, object> _func;
    private readonly string _expressionView;

    internal GetOperation(Func<object, object, object> func, string expressionView = "")
    {
        _func = func;
        _expressionView = expressionView;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        runContext.Value = _func(runContext.Context, runContext.Value);
    }

    public override string ToString()
    {
        return "GET " + _expressionView;
    }
}
