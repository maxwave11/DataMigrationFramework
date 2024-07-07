using System.Collections.Generic;
using System.Linq;
using DataMigrationCore.Logging;
using DataMigrationCore.Pipeline.Operations;

namespace DataMigrationCore.Pipeline.Pipes;

internal class PipeConfig : IPipeConfig
{
    private readonly List<IOperationConfig> _operationConfigs = new();

    public void AddOperationConfig(IOperationConfig operationConfig)
    {
        _operationConfigs.Add(operationConfig);
    }

    public IPipe CreatePipe(IMigrationLogger logger)
    {
        var operations = _operationConfigs.Select(config => config.CreateOperation(logger)).ToArray();
        return new Pipe(operations, logger);
    }
}
