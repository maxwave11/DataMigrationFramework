namespace DataMigrationCore.Data;

public class DefaultPipelineContext<TSource, TTarget>
{
    public TSource Source { get; init; }
    public TTarget Target { get; set; }
}
