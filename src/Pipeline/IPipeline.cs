using DataMigrationCore.Job;

namespace DataMigrationCore.Pipeline;

public interface IPipeline : IJob
{
    void ConfigurePipeline(params object[] args);
}

