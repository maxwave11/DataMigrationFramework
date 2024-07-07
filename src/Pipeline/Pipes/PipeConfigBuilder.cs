using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DataMigrationCore.Enums;
using DataMigrationCore.Pipeline.Operations;

namespace DataMigrationCore.Pipeline.Pipes;

public class PipeConfigBuilder<TContext, TValue>
{
    private readonly IPipeConfig _pipeConfig;

    public PipeConfigBuilder(IPipeConfig pipeConfig)
    {
        _pipeConfig = pipeConfig;
    }

    internal PipeConfigBuilder<TContext, TNextOutput> AddOperationConfig<TContext, TNextOutput>(
        IOperationConfig operationConfig)
    {
        _pipeConfig.AddOperationConfig(operationConfig);
        return new PipeConfigBuilder<TContext, TNextOutput>(_pipeConfig);
    }


    public PipeConfigBuilder<TContext, TNextValue> GET<TNextValue>(
        Func<(TContext Context, TValue Value), TNextValue> func,
        [CallerArgumentExpression("func")] string expression = "")
    {
        var nextOperationConfig = new GetOperationConfig(
            (ctx, value) => func(((TContext)ctx, (TValue)value)),
            expression);

        return AddOperationConfig<TContext, TNextValue>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, TValue> SET(
        Action<(TContext Context, TValue Value)> action,
        [CallerArgumentExpression("action")] string expression = "")
    {
        var nextOperationConfig = new SetOperationConfig(
            (ctx, value) => action(((TContext)ctx, (TValue)value)),
            expression);

        return AddOperationConfig<TContext, TValue>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, TValue> IF(
        Func<(TContext Context, TValue Value), bool> condition,
        PipelineFlowControl ifFalseFlowControl = PipelineFlowControl.SkipPipe,
        string ifFalseMessage = null,
        [CallerArgumentExpression("condition")] string expression = "")
    {
        var nextOperationConfig = new IfOperationConfig(
            (ctx, value) => condition(((TContext)ctx, (TValue)value)),
            ifFalseFlowControl,
            expression);

        return AddOperationConfig<TContext, TValue>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, TLookupItem> LOOKUP<TLookupItem>(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> dataKeySelector,
        LookupMode lookupMode = LookupMode.Single,
        bool enableCache = true,
        PipelineFlowControl onNotFound = PipelineFlowControl.Stop,
        [CallerArgumentExpression("dataSource")] string dataSourceExpression = "")
    {
        var nextOperationConfig = new LookupOperationConfig<TLookupItem>(
            dataSource: dataSource,
            keySelector: dataKeySelector,
            lookupMode: lookupMode,
            enableCache: enableCache,
            onNotFound: onNotFound,
            dataSourceExpressionView: dataSourceExpression);

        return AddOperationConfig<TContext, TLookupItem>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, IEnumerable<TLookupItem>> LOOKUP_MANY<TLookupItem>(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> dataKeySelector,
        StringComparison keyComparisonType = StringComparison.InvariantCultureIgnoreCase,
        [CallerArgumentExpression("dataSource")] string dataSourceExpression = "")
    {
        var nextOperationConfig = new LookupOperationConfig<TLookupItem>(
            dataSource: dataSource,
            keySelector: dataKeySelector,
            lookupMode: LookupMode.All,
            keyComparisonType: keyComparisonType,
            dataSourceExpressionView: dataSourceExpression);

        return AddOperationConfig<TContext, IEnumerable<TLookupItem>>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, TLookupItem> LOOKUP_OR_CREATE<TLookupItem>(
        IEnumerable<TLookupItem> dataSource,
        Func<TLookupItem, string> dataKeySelector,
        Func<TContext, TLookupItem> newObjectSelector = null,
        LookupMode lookupMode = LookupMode.Single,
        [CallerArgumentExpression("dataSource")] string dataSourceExpression = "")
        where TLookupItem : new()
    {
        var nextOperationConfig = new LookupOrCreateOperationConfig<TContext, TLookupItem>(
            dataSource: dataSource,
            keySelector: dataKeySelector,
            newObjectSelector: newObjectSelector,
            lookupMode: lookupMode,
            dataSourceExpressionView: dataSourceExpression);

        return AddOperationConfig<TContext, TLookupItem>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, TValue> ADD(
        Func<(TContext Context, TValue Value), object> func,
        [CallerArgumentExpression("func")] string expression = "")
    {
        var nextOperationConfig = new AddToResultsOperationConfig(
            (ctx, value) => new[] { func(((TContext)ctx, (TValue)value)) },
            expression);

        return AddOperationConfig<TContext, TValue>(nextOperationConfig);
    }

    public PipeConfigBuilder<TContext, TValue> ADD_MANY(
        Func<(TContext Context, TValue Value), IReadOnlyCollection<object>> func,
        [CallerArgumentExpression("func")] string expression = "")
    {
        var nextOperationConfig = new AddToResultsOperationConfig(
            (ctx, value) => func(((TContext)ctx, (TValue)value)),
            expression);

        return AddOperationConfig<TContext, TValue>(nextOperationConfig);
    }

    public PipeConfigBuilder<TNewContext, TValue> WITH_CONTEXT<TNewContext>(
        Func<(TContext Context, TValue Value), TNewContext> func,
        [CallerArgumentExpression("func")] string expression = "")
    {
        var nextOperationConfig = new ChangeContextOperationConfig<TNewContext>(
            (ctx, value) => func(((TContext)ctx, (TValue)value)),
            expression);

        return AddOperationConfig<TNewContext, TValue>(nextOperationConfig);
    }
}
