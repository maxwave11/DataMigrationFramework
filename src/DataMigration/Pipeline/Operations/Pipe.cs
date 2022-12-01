using System;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Operations;

public class SetOperation<TContext, TOutput>: IOperation<TContext, TOutput> 
    where TContext: IPipeContext
{
    public IPipeContext Context { get; }
    public IOperation NextOperation { get; set; }

    private readonly Action<TContext> _action;

    public SetOperation(Action<TContext> action, TContext context)
    {
        _action = action;
        Context = context;
    }
    
    public object Execute()
    {
        _action((TContext)Context);
        return Context.GetValue();
    }
    
    public override string ToString()
    {
        return "SET";
    }
}

public class LogOperation<TContext, TOutput>: IOperation<TContext, TOutput> 
    where TContext: IPipeContext
{
    private readonly Func<TContext, string> _message;
    private readonly ConsoleColor _color;
    private readonly TraceMode _level;
    public IPipeContext Context { get; }
    public IOperation NextOperation { get; set; }


    public LogOperation(Func<TContext, string> message, ConsoleColor color, TContext context, TraceMode level)
    {
        _message = message;
        _color = color;
        _level = level;
        Context = context;
    }
    
    public object Execute()
    {
        var message = _message((TContext)Context);
        Migrator.Current.Tracer.TraceLine(message, null, _level, _color);
        return Context.GetValue();
    }
    
    public override string ToString()
    {
        return "LOG";
    }
}

