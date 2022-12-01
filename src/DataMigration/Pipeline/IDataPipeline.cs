using DataMigration.Trace;

namespace DataMigration.Pipeline;

public interface IDataPipeline
{
    void Initialize(IMigrationTracer tracer);
    void Run();
}