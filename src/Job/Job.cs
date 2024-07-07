using System.Threading;
using System.Threading.Tasks;
using DataMigrationCore.Logging;
using Microsoft.Extensions.Logging;

namespace DataMigrationCore.Job;

public abstract class Job : IJob
{
    public bool Enabled { get; set; } = true;
    public string Name { get; set; }
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public abstract Task Run(IMigrationLogger logger, CancellationToken cancellationToken);
}
