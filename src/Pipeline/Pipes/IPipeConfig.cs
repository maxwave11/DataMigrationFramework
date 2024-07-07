using DataMigrationCore.Logging;
using DataMigrationCore.Pipeline.Operations;

namespace DataMigrationCore.Pipeline.Pipes;

public interface IPipeConfig
{
    void AddOperationConfig(IOperationConfig operationConfig);
    IPipe CreatePipe(IMigrationLogger logger);
}
