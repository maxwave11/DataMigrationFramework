namespace DataMigration.Pipeline;

public interface IPipe
{
    void Execute(ValueTransitContext ctx);
}