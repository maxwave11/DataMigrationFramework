using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataMigrationCore.Data;
using DataMigrationCore.Data.Interfaces;
using DataMigrationCore.Logging;
using DataMigrationCore.Pipeline.Operations;
using DataMigrationCore.Pipeline.Pipes;

namespace DataMigrationCore.Pipeline;


public abstract class Pipeline<TContext> : Job.Job, IPipeline
{
    public IEnumerable<TContext> Source { get; set; }
    public int CommitEach { get; set; } = 100;
    public IDataTarget DataTarget { get; set; }

    protected IMigrationLogger _logger;

    private readonly bool _saveAsync = false;
    private Task _savingTask = Task.CompletedTask;
    private readonly List<IPipeConfig> _pipeConfigs = new();

    public abstract void ConfigurePipeline(params object[] args);

    internal virtual IPipelineSource GetPipelineSource()
    {
        return new PipelineSource<TContext>(Source, _logger);
    }
    public override async Task Run(IMigrationLogger logger, CancellationToken cancellationToken)
    {
        _logger = logger;
        var totalObjectsHandled = 0;

        logger.LogInformation("Getting total amount of source objects...");

        var pipelineSource = GetPipelineSource();
        var totalObjectsCount = pipelineSource.Count();

        logger.LogInformation($"Source objects count = {totalObjectsCount}");

        List<object> pageObjectsToUpdate = new(CommitEach);
        List<object> pageObjectsToAdd = new(CommitEach);
        var pipes = _pipeConfigs.Select(config => config.CreatePipe(logger)).ToList();

        var timer = new Stopwatch();
        timer.Start();

        foreach (var context in pipelineSource)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.ClearLastLogs();

            using var objectLoggingScope = logger.BeginScope($"Object [{++totalObjectsHandled}/{totalObjectsCount}]");

            var pipelineRun = new PipelineRun(context, pipes, logger);
            var runResults = pipelineRun.Run(cancellationToken);

            pageObjectsToAdd.AddRange(runResults.ObjectsToAdd);
            pageObjectsToUpdate.AddRange(runResults.ObjectsToUpdate);

            if (totalObjectsHandled % CommitEach == 0)
            {
                var results = new PipelineResults(pageObjectsToAdd, pageObjectsToUpdate, timer.Elapsed);
                timer.Restart();

                await CommitChanges(results, false, cancellationToken);

                pageObjectsToAdd = new();
                pageObjectsToUpdate = new();
            }
        }

        await CommitChanges(new PipelineResults(pageObjectsToAdd, pageObjectsToUpdate, timer.Elapsed), true, cancellationToken);
    }


    private async Task CommitChanges(PipelineResults results, bool isLastCommit, CancellationToken cancelToken)
    {
        // Wait previous committing task before starting new one
        await _savingTask;

        if (!results.ObjectsToAdd.Any() && !results.ObjectsToUpdate.Any())
            return;

        if (DataTarget == null)
            throw new InvalidOperationException($"DataTarget isn't configured");

        _savingTask = ProcessCommitChanges(results, cancelToken);

        if (!_saveAsync || isLastCommit)
            await _savingTask;
    }

    private async Task ProcessCommitChanges(PipelineResults results, CancellationToken cancelToken)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        _logger.SetResults(results);

        await DataTarget.CommitChangesAsync(results, cancelToken);

        _logger.LogInformation(
            $"Processed objects. ToAdd: {results.ObjectsToAdd.Count}, ToUpdate: {results.ObjectsToUpdate.Count}. " +
            $"Handled in {results.ElapsedTime.TotalSeconds} sec. Saved in {stopWatch.Elapsed.TotalSeconds} sec.");
    }

    public PipeConfigBuilder<TContext, object> PIPE(string pipeName)
    {
        var startOperationConfig = new PipeStartOperationConfig(pipeName);
        var pipeConfig = new PipeConfig();
        pipeConfig.AddOperationConfig(startOperationConfig);

        _pipeConfigs.Add(pipeConfig);

        return new PipeConfigBuilder<TContext, object>(pipeConfig);
    }
}

public abstract class Pipeline<TSource, TTarget> : Pipeline<DefaultPipelineContext<TSource, TTarget>>
{
    public new IEnumerable<TSource> Source { get; set; }

    internal override IPipelineSource GetPipelineSource()
    {
        return new PipelineSource<TSource>(Source, _logger, item =>
            new DefaultPipelineContext<TSource, TTarget> { Source = item });
    }
}

