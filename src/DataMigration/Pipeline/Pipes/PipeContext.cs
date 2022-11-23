using DataMigration.Data;

namespace DataMigration.Pipeline.Pipes;

public interface IPipeContext
{
   
}

public class PipeContext<TSource, TTarget, TValue>: IPipeContext
    where TSource: IDataObject
    where TTarget: IDataObject
{
    public TValue PipeValue { get; }
    public TSource Source { get; }
    public TTarget Target { get; }
    
    public PipeContext(TValue pipeValue,TSource source, TTarget target)
    {
        Source = source;
        Target = target;
        PipeValue = pipeValue;
    }
}