using System;
using System.Linq.Expressions;
using DataMigration.Data;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Pipes;



public class Pipe<TContext, TOutput>: IPipe<TContext, TOutput> where TContext: IPipeContext
{
    public IPipe PreviousPipe { get; }
    public object Execute(object pipeValue, IDataObject source, IDataObject target)
    {
        throw new NotImplementedException();
    }

    private readonly Delegate _func;
    private readonly LambdaExpression _expression;
    
    // private readonly Func<PipeContext<TInput>, TOutput> _func;
    // private Expression<Func<PipeContext<TInput>, TOutput>> _expression { get; }
    
    public Pipe(LambdaExpression expression, IPipe previousPipe)
    {
        PreviousPipe = previousPipe;

        _expression = expression;
        _func = expression.Compile();
    }

    // public object Execute(object pipeValue, IDataObject source, IDataObject target)
    // {
    //     var concretePipeValue = default(TInput);
    //         
    //     if (pipeValue != null)
    //     {
    //         if (pipeValue is not TInput input)
    //             throw new InvalidCastException(
    //                 $"Can't convert pipeValue of type {pipeValue.GetType()} to {typeof(TInput)}");
    //
    //         concretePipeValue = input;
    //     }
    //     
    //     var pipeContext = new PipeContext<TInput>(concretePipeValue, source, target);
    //     
    //     var result = _func.DynamicInvoke(pipeContext);
    //     return result;
    // }
    
    public object Execute(TContext context)
    {
        var result = _func.DynamicInvoke(context);
        return result;
    }
    
    public override string ToString()
    {
        return _expression.GetExpressionText();
    }
}

