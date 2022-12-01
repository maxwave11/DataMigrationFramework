namespace DataMigration.Pipeline.Operations;

public interface IOperation
{
    IPipeContext Context { get; }
    IOperation NextOperation { get; set; }
    object Execute();
}

public interface IOperation<TContext, TOutput> : IOperation
{
}