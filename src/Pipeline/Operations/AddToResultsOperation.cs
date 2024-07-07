using System;
using System.Collections.Generic;
using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class AddToResultsOperationConfig : IOperationConfig
{
    private readonly Func<object, object, IReadOnlyCollection<object>> _func;
    private readonly string _expressionView;

    internal AddToResultsOperationConfig(Func<object, object, IReadOnlyCollection<object>> func, string expressionView)
    {
        _func = func;
        _expressionView = expressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new AddToResultsOperation(_func, logger, _expressionView);
    }
}

internal class AddToResultsOperation : IOperation
{
    private readonly Func<object, object, IReadOnlyCollection<object>> _func;
    private readonly IMigrationLogger _logger;
    private readonly string _expressionView;

    public AddToResultsOperation(Func<object, object, IReadOnlyCollection<object>> func,
        IMigrationLogger logger, string expressionView)
    {
        _func = func;
        _logger = logger;
        _expressionView = expressionView;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        var objectsToAdd = _func(runContext.Context, runContext.Value);
        foreach (var objectToAdd in objectsToAdd)
        {
            _logger.LogTrace($"pushed {objectToAdd}");
            runContext.PushNewObjectToResults(objectToAdd);
        }
    }

    public override string ToString()
    {
        return "PUSH " + _expressionView;
    }
}
