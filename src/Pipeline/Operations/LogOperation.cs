using System;
using System.Threading;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class LogOperation : IOperation
{
    private readonly Func<object, object, string> _message;
    private readonly IMigrationLogger _logger;

    public LogOperation(Func<object, object, string> message, IMigrationLogger logger)
    {
        _message = message;
        _logger = logger;
    }

    public void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        var message = _message(runContext.Context, runContext.Value);
        _logger.LogInformation(message);
    }

    public override string ToString()
    {
        return "LOG";
    }
}
