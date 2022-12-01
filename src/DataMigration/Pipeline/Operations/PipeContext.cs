using System;
using DataMigration.Data;
using DataMigration.Data.Interfaces;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Operations;

public interface IPipeContext
{
    PipelineFlowControl FlowControl { get; set; }
    public IPipeContext Previous { get; }

    string Message { get; set; }
    object GetValue();
    void Set(object pipeValue, IDataObject source, IDataObject target);
}

public class PipeContext<TSource, TTarget, TValue>: IPipeContext
    where TSource: IDataObject
    where TTarget: IDataObject
{
    public TValue Value { get; private set; }
    public TSource Source { get; private set;  }
    public TTarget Target { get; private set;  }
    
    public IPipeContext Previous { get; }

    public PipelineFlowControl FlowControl { get; set; }
    public string Message { get; set; }

    public PipeContext(IPipeContext previousContext)
    {
        Previous = previousContext;
    }

    public virtual void Set(object pipeValue, IDataObject source, IDataObject target)
    {
        Source = (TSource)source;
        Target = (TTarget)target;
        SetValue(pipeValue);
    }
    
    public virtual void SetValue(object pipeValue)
    {
        var concretePipeValue = default(TValue);
            
        if (pipeValue != null)
        {
            if (pipeValue is not TValue input)
                throw new InvalidCastException(
                    $"Can't convert pipeValue of type {pipeValue.GetType()} to {typeof(TValue)}");
        
            concretePipeValue = input;
        }
        
        Value = concretePipeValue;
    }

    public void SetSource(IDataObject source)
    {
        Source = (TSource)source;
    }

    public void SetTarget(IDataObject target)
    {
        Target = (TTarget)target;
    }

    public virtual object GetValue()
    {
        return Value;
    }
}