using System.Threading;
using System.Threading.Tasks;
using DataMigrationCore.Logging;
using Microsoft.Extensions.Logging;

namespace DataMigrationCore.Job;

public interface IJob
{
    bool Enabled { get; set; }
    string Name { get; set; }
    LogLevel LogLevel { get; set; }
    Task Run(IMigrationLogger logger, CancellationToken cancellationToken);
}
