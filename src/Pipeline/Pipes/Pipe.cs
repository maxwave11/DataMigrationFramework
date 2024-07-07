using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DataMigrationCore.Enums;
using DataMigrationCore.Logging;
using DataMigrationCore.Pipeline.Operations;
using DataMigrationCore.Utils;

namespace DataMigrationCore.Pipeline.Pipes;

internal class Pipe : IPipe
{
    private readonly IReadOnlyCollection<IOperation> _operations;
    private readonly IMigrationLogger _logger;

    public Pipe(IReadOnlyCollection<IOperation> operations, IMigrationLogger logger)
    {
        _operations = operations;
        _logger = logger;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        _logger.LogTrace("");

        foreach (var operation in _operations)
        {
            var isFirstOperation = _operations.First() == operation;

            if (!isFirstOperation)
                _logger.Indent();

            _logger.LogTrace($"{operation}");
            _logger.Indent();

            operation.Execute(runContext, cancellationToken);

            _logger.IndentBack();

            if (!isFirstOperation)
            {
                TraceValueInfo(runContext.Value);
                _logger.IndentBack();
            }

            if (runContext.FlowControl == PipelineFlowControl.Debug)
                Debugger.Break();

            if (runContext.FlowControl == PipelineFlowControl.Stop)
                throw new Exception("Migration stopped");

            if (runContext.FlowControl == PipelineFlowControl.SkipObject)
            {
                _logger.LogInformation($"FLOW: {runContext.FlowControl}");
                break;
            }

            if (runContext.FlowControl != PipelineFlowControl.Continue)
            {
                _logger.LogTrace($"FLOW: {runContext.FlowControl}");
                break;
            }
        }
    }

    private void TraceValueInfo(object value)
    {
        var valueType = value?.GetType().Name.Truncate(30);
        var value1 = value?.ToString().Truncate(240) ?? "null";
        _logger.LogTrace($" => ({valueType}){value1}");
    }
}
