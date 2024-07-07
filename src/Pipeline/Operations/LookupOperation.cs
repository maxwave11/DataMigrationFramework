using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataMigrationCore.Data;
using DataMigrationCore.Enums;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class LookupOperationConfig<TLookupItem> : IOperationConfig
{
    private readonly IEnumerable<TLookupItem> _dataSource;
    private readonly Func<TLookupItem, string> _keySelector;
    private readonly LookupMode _lookupMode;
    private readonly bool _enableCache;
    private readonly StringComparison _keyComparisonType;
    private readonly PipelineFlowControl _onNotFound;
    private readonly string _dataSourceExpressionView;

    internal LookupOperationConfig(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> keySelector,
        LookupMode lookupMode,
        bool enableCache = true,
        StringComparison keyComparisonType = StringComparison.InvariantCultureIgnoreCase,
        PipelineFlowControl onNotFound = PipelineFlowControl.Stop,
        string dataSourceExpressionView = "")
    {
        _dataSource = dataSource;
        _keySelector = keySelector;
        _lookupMode = lookupMode;
        _enableCache = enableCache;
        _keyComparisonType = keyComparisonType;
        _onNotFound = onNotFound;
        _dataSourceExpressionView = dataSourceExpressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new LookupOperation<TLookupItem>(
            _dataSource,
            _keySelector,
            logger,
            _lookupMode,
            _enableCache,
            _keyComparisonType,
            _onNotFound,
            _dataSourceExpressionView);
    }
}


internal class LookupOperation<TLookupItem> : IOperation
{
    private readonly IMigrationLogger _logger;
    private readonly string _dataSourceExpressionView;
    protected readonly LookupDataSource<TLookupItem> LookupDataSource;

    private readonly LookupMode _mode;

    private readonly PipelineFlowControl _onNotFound = PipelineFlowControl.Stop;

    public LookupOperation(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> keySelector,
        IMigrationLogger logger,
        LookupMode lookupMode,
        bool enableCache = true,
        StringComparison keyComparisonType = StringComparison.InvariantCultureIgnoreCase,
        PipelineFlowControl onNotFound = PipelineFlowControl.Stop,
        string dataSourceExpressionView = "")
    {
        _logger = logger;
        _dataSourceExpressionView = dataSourceExpressionView;
        _mode = lookupMode;
        _onNotFound = onNotFound;

        LookupDataSource = new LookupDataSource<TLookupItem>(dataSource, keySelector, logger, enableCache, keyComparisonType);
    }

    public virtual void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        var keyToFind = runContext.Value?.ToString();

        object lookupObject = FindLookupObjectsByKey(keyToFind, runContext);

        if (lookupObject == null)
        {
            _logger.LogTrace("Lookup object not found by key " + keyToFind);
            runContext.FlowControl = _onNotFound;
        }
        else
        {
            runContext.PushUpdatedObjectToResults(lookupObject);
        }

        runContext.Value = lookupObject;
    }

    protected object FindLookupObjectsByKey(string key, PipelineRunContext runContext)
    {
        var foundObjects = LookupDataSource.GetObjectsByKey(key).ToList();

        switch (_mode)
        {
            case LookupMode.Single:
                if (foundObjects.Count > 1)
                    throw new InvalidOperationException(
                        message: $"Found multiple objects by key = {key}");
                return foundObjects.SingleOrDefault();
            case LookupMode.First:
                return foundObjects.FirstOrDefault();
            case LookupMode.All:
                return foundObjects.Any() ? foundObjects : null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string ToString()
    {
        return "LOOKUP " + _dataSourceExpressionView;
    }
}

