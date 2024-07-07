using System;
using System.Collections.Generic;
using System.Threading;
using DataMigrationCore.Enums;
using DataMigrationCore.Logging;

namespace DataMigrationCore.Pipeline.Operations;

internal class LookupOrCreateOperationConfig<TContext, TLookupItem> : IOperationConfig
    where TLookupItem : new()
{
    private readonly IEnumerable<TLookupItem> _dataSource;
    private readonly Func<TLookupItem, string> _keySelector;
    private readonly Func<TContext, TLookupItem> _newObjectSelector;
    private readonly LookupMode _lookupMode;
    private readonly PipelineFlowControl _onNotFound;
    private readonly string _dataSourceExpressionView;

    internal LookupOrCreateOperationConfig(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> keySelector,
        LookupMode lookupMode,
        Func<TContext, TLookupItem> newObjectSelector = null,
        PipelineFlowControl onNotFound = PipelineFlowControl.Stop,
        string dataSourceExpressionView = "")
    {
        _dataSource = dataSource;
        _keySelector = keySelector;
        _newObjectSelector = newObjectSelector;
        _lookupMode = lookupMode;
        _onNotFound = onNotFound;
        _dataSourceExpressionView = dataSourceExpressionView;
    }

    public IOperation CreateOperation(IMigrationLogger logger)
    {
        return new LookupOrCreateOperation<TContext, TLookupItem>(
            _dataSource,
            _keySelector,
            logger,
            _newObjectSelector,
            _lookupMode,
            _dataSourceExpressionView);
    }
}


internal class LookupOrCreateOperation<TContext, TLookupItem> : LookupOperation<TLookupItem>
    where TLookupItem : new()
{
    private readonly IMigrationLogger _logger;
    private readonly Func<TContext, TLookupItem> _newObjectSelector;
    private readonly string _dataSourceExpression;

    public LookupOrCreateOperation(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> keySelector,
        IMigrationLogger logger,
        Func<TContext, TLookupItem> newObjectSelector = null,
        LookupMode lookupMode = LookupMode.Single,
        string dataSourceExpression = "")
        : base(dataSource, keySelector, logger, lookupMode)
    {
        _logger = logger;
        _newObjectSelector = newObjectSelector;
        _dataSourceExpression = dataSourceExpression;
    }

    public override void Execute(PipelineRunContext runContext, CancellationToken cancellationToken)
    {
        var keyToFind = runContext.Value?.ToString();

        object lookupObject = FindLookupObjectsByKey(keyToFind, runContext);

        if (lookupObject == null)
        {
            lookupObject = _newObjectSelector == null ? new TLookupItem() : _newObjectSelector((TContext)runContext.Context);
            LookupDataSource.AddToCache(keyToFind, (TLookupItem)lookupObject);
            runContext.PushNewObjectToResults(lookupObject);
            _logger.LogTrace($"Created new object: Key {keyToFind}, Type: {typeof(TLookupItem)}");
        }
        else
        {
            _logger.LogTrace($"Found existed object: Key {keyToFind}, Type: {typeof(TLookupItem)}");
            runContext.PushUpdatedObjectToResults(lookupObject);
        }

        runContext.Value = lookupObject;
    }

    public override string ToString()
    {
        return "LOOKUP_OR_CREATE " + _dataSourceExpression;
    }
}
