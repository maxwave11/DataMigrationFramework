using System;
using System.Collections.Generic;
using DataMigration.Data;
using DataMigration.Data.Interfaces;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Operations;

public static class OperationsExtensions
{
    public static GetOperation<PipeContext<TSource, TTarget, TOutput>, TNextOutput> GET<TSource, TTarget, TInput, TOutput,
        TNextOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        Func<PipeContext<TSource, TTarget, TOutput>, TNextOutput> func)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe =  new GetOperation<PipeContext<TSource, TTarget, TOutput>, TNextOutput>(func, context);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static IOperation<PipeContext<TSource, TTarget, TOutput>, TOutput> SET<TSource, TTarget, TInput, TOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        Action<PipeContext<TSource, TTarget, TOutput>> action)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe = new SetOperation<PipeContext<TSource, TTarget, TOutput>, TOutput>(action, context);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static LookupOperation<PipeContext<TSource, TTarget, TOutput>, TNextOutput, TNextOutput> LOOKUP<TSource, TTarget, TInput, TOutput, TNextOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        ICachedDataSource<TNextOutput> dataSource,
        LookupMode lookupMode = LookupMode.Single,
        bool createIfNotFound = false)
        where TSource : IDataObject
        where TTarget : IDataObject
        where TNextOutput : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe = new LookupOperation<PipeContext<TSource, TTarget, TOutput>, TNextOutput,TNextOutput>(
            context: context,
            dataSource: dataSource, 
            lookupMode: lookupMode, 
            lookupPredicate: null,
            createIfNotFound);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static LookupOperation<PipeContext<TSource, TTarget, TOutput>, TNextOutput, IEnumerable<TNextOutput>> LOOKUP_MANY<TSource, TTarget, TInput, TOutput, TNextOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        ICachedDataSource<TNextOutput> dataSource)
        where TSource : IDataObject
        where TTarget : IDataObject
        where TNextOutput : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe =  new LookupOperation<PipeContext<TSource, TTarget, TOutput>, TNextOutput, IEnumerable<TNextOutput>>(
            context: context,
            dataSource: dataSource, 
            lookupMode: LookupMode.All, 
            lookupPredicate: null);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static IfOperation<PipeContext<TSource, TTarget, TOutput>, TOutput> IF<TSource, TTarget, TInput, TOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        Predicate<PipeContext<TSource, TTarget, TOutput>> predicate,
        PipelineFlowControl ifFalseFlowControl = PipelineFlowControl.SkipValue,
        string ifFalseMessage = null)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe =  new IfOperation<PipeContext<TSource, TTarget, TOutput>, TOutput>(predicate, context, ifFalseFlowControl, ifFalseMessage);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static FlowOperation<PipeContext<TSource, TTarget, TOutput>, TOutput> FLOW<TSource, TTarget, TInput, TOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation, 
        PipelineFlowControl flow, string message = "")
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe =  new FlowOperation<PipeContext<TSource, TTarget, TOutput>, TOutput>(flow, context, message);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static LogOperation<PipeContext<TSource, TTarget, TOutput>, TOutput> LOG<TSource, TTarget, TInput, TOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        Func<PipeContext<TSource, TTarget, TOutput>, string> func,
         TraceMode level = TraceMode.Object, ConsoleColor logColor = ConsoleColor.Green)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe =  new LogOperation<PipeContext<TSource, TTarget, TOutput>, TOutput>(func, logColor, context, level);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
    
    public static LogOperation<PipeContext<TSource, TTarget, TOutput>, TOutput> LOG<TSource, TTarget, TInput, TOutput>(
        this IOperation<PipeContext<TSource, TTarget, TInput>, TOutput> operation,
        string message, TraceMode level = TraceMode.Object, ConsoleColor logColor = ConsoleColor.Green)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        var context = new PipeContext<TSource, TTarget, TOutput>(operation.Context);
        var nextPipe =  new LogOperation<PipeContext<TSource, TTarget, TOutput>, TOutput>(_ => message, ConsoleColor.Green, context, level);
        operation.NextOperation = nextPipe;
        return nextPipe;
    }
}