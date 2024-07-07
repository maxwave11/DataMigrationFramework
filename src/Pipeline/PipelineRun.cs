using System.Collections.Generic;
using System.Threading;
using DataMigrationCore.Data;
using DataMigrationCore.Enums;
using DataMigrationCore.Logging;
using DataMigrationCore.Pipeline.Pipes;

namespace DataMigrationCore.Pipeline;

internal class PipelineRun
{
    private readonly object _context;
    private readonly IReadOnlyCollection<IPipe> _pipes;

    private readonly IMigrationLogger _logger;

    public PipelineRun(object context, IReadOnlyCollection<IPipe> pipes, IMigrationLogger logger)
    {
        _context = context;
        _pipes = pipes;
        _logger = logger;
    }

    public PipelineResults Run(CancellationToken cancellationToken)
    {
        var runContext = new PipelineRunContext(_context);

        _logger.SetContext(_context);

        _logger.Indent();

        RunPipes(runContext, cancellationToken);

        if (runContext.FlowControl == PipelineFlowControl.SkipObject)
        {
            _logger.LogInformation("Object skipped");
            return new PipelineResults();
        }

        _logger.IndentBack();

        return runContext.GetResults();
    }

    private void RunPipes(PipelineRunContext ctx, CancellationToken cancellationToken)
    {
        foreach (var pipe in _pipes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pipe.Execute(ctx, cancellationToken);

            if (ctx.FlowControl == PipelineFlowControl.SkipPipe)
            {
                ctx.FlowControl = PipelineFlowControl.Continue;
                continue;
            }

            if (ctx.FlowControl != PipelineFlowControl.Continue)
                break;
        }
    }
}
