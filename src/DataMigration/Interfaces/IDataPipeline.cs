namespace DataMigration.Pipeline;

public interface IDataPipeline
{
    void Initialize();
    void Run();
}