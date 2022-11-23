using DataMigration.Data;

namespace DataMigration.Pipeline.Pipes;

public interface IPipe
{
    IPipe PreviousPipe { get; }
    object Execute(object pipeValue, IDataObject source, IDataObject target);

}

public interface IPipe<TContext, TOutput> : IPipe
{
}