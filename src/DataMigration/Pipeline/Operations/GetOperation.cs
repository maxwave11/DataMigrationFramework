using System;

namespace DataMigration.Pipeline.Operations;

public class GetOperation<TContext, TOutput> : IOperation<TContext, TOutput> where TContext: IPipeContext
{
    public IPipeContext Context { get; }
    public IOperation NextOperation { get; set; }
    
    private readonly Func<TContext, TOutput> _func;

    public GetOperation(Func<TContext, TOutput> func, TContext context)
    {
        _func = func;
        Context = context;
    }
    
    public object Execute()
    {
        return _func((TContext)Context);
    }

    public override string ToString()
    {
        return "GET";
    }
}