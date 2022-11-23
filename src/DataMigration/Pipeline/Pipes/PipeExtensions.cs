using System;
using System.Linq.Expressions;
using DataMigration.Data;
using DataMigration.Data.DataSources;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Pipes;

public static class PipeExtensions
{
    public static IPipe<PipeContext<TSource, TTarget, object>, TOutput> GET<TSource, TTarget, TOutput>(
        this DataPipeline<TSource, TTarget> dataPipeline,
        Expression<Func<PipeContext<TSource, TTarget, object>, TOutput>> expression)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        return new Pipe<PipeContext<TSource, TTarget, object>, TOutput>(expression, null);
    }

    public static Pipe<PipeContext<TSource, TTarget, TOutput>, TNextOutput> GET<TSource, TTarget, TInput, TOutput,
        TNextOutput>(
        this IPipe<PipeContext<TSource, TTarget, TInput>, TOutput> pipe,
        Expression<Func<PipeContext<TSource, TTarget, TOutput>, TNextOutput>> expression)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        return new Pipe<PipeContext<TSource, TTarget, TOutput>, TNextOutput>(expression, pipe);
    }
    
    public static Pipe<PipeContext<TSource, TTarget, TOutput>, TOutput> SET<TSource, TTarget, TInput, TOutput>(
        this IPipe<PipeContext<TSource, TTarget, TInput>, TOutput> pipe,
        Expression<Action<PipeContext<TSource, TTarget, TOutput>>> expression)
        where TSource : IDataObject
        where TTarget : IDataObject
    {
        return new Pipe<PipeContext<TSource, TTarget, TOutput>, TOutput>(expression, pipe);
    }
    
    public static LookupPipe<PipeContext<TSource, TTarget, TOutput>, TNextOutput> LOOKUP<TSource, TTarget, TInput, TOutput, TNextOutput>(
        this IPipe<PipeContext<TSource, TTarget, TInput>, TOutput> pipe,
        IDataSource<TNextOutput> dataSource,
        LookupMode lookupMode = LookupMode.Single 
        )
        where TSource : IDataObject
        where TTarget : IDataObject
        where TNextOutput : IDataObject
    {
        
        return new LookupPipe<PipeContext<TSource, TTarget, TOutput>,TNextOutput>(
            previousPipe: pipe, 
            dataSource: (ICachedDataSource)dataSource, 
            lookupMode: lookupMode, 
            lookupPredicate: null);
        
        
        //return new Pipe<PipeContext<TSource, TTarget, TOutput>, TNextOutput>(expression, pipe);
    }

   

    // public static Pipe<TOutput, TOutput> SET<TInput, TOutput>( 
    //     this IPipe<TInput,TOutput> pipe,
    //     Expression<Action<PipeContext<TOutput>>> expression)
    // {
    //     return new Pipe<TOutput, TOutput>(expression, pipe);
    // }
    
    // public static LookupPipe<TOutput, TNextOutput> LOOKUP<TInput, TOutput, TNextOutput>(
    //     this IPipe<TInput, TOutput> pipe,
    //     IDataSource<TNextOutput> dataSource,
    //     LookupMode lookupMode = LookupMode.Single 
    //     ) where TNextOutput : IDataObject
    // {
    //     return new LookupPipe<TOutput,TNextOutput>(
    //         previousPipe: pipe, 
    //         dataSource: (ICachedDataSource)dataSource, 
    //         lookupMode: lookupMode, 
    //         lookupPredicate: null);
    //     
    // }
}