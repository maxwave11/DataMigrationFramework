using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DataMigrationCore.Logging;
using DataMigrationCore.Utils;
using Microsoft.Extensions.Logging;

namespace DataMigrationCore.Job;

public class JobRunner
{
    private readonly IJob _job;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _correlationId;

    public JobRunner(IJob job, ILoggerFactory loggerFactory, string correlationId)
    {
        _job = job;
        _loggerFactory = loggerFactory;
        _correlationId = correlationId;
    }

    public async Task Run(CancellationToken cancelToken)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var loggerCategoryPrefix = _correlationId.IsNotEmpty() ? "|" + _correlationId : "";
        var nativeLogger = _loggerFactory.CreateLogger(_job.GetType() + loggerCategoryPrefix);
        var logger = new MigrationLogger(nativeLogger, _job.LogLevel);

        using var scope = logger.BeginScope(_job.Name);

        if (!_job.Enabled)
        {
            logger.LogInformation($"Job '{_job.Name}' Disabled");
            return;
        }

        logger.LogInformation($"Job '{_job.Name}' started. " +
                              $"ThreadId = {Thread.CurrentThread.ManagedThreadId}, IsPoolThread={Thread.CurrentThread.IsThreadPoolThread}");
        logger.Indent();

        try
        {
            await _job.Run(logger, cancelToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.TraceException(e, _correlationId);
            throw;
        }

        logger.IndentBack();
        logger.LogInformation($"Job finished in {stopWatch.Elapsed.TotalSeconds} sec");
    }
}
