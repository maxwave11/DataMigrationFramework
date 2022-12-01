using System;
using System.Diagnostics;
using DataMigration.Enums;
using DataMigration.Pipeline;
using DataMigration.Trace;

namespace DataMigration;

public class Migrator
{
    internal IMigrationTracer Tracer { get; private set; }
        
    public IDataPipeline[] Pipelines { get; set; }

    public static Migrator Current  {get; private set; }
    public bool BreakOnError { get; }

    
    public Migrator(IMigrationTracer tracer, TraceMode traceMode)
    {
        Current = this;
        Tracer = tracer;
    }

    public void Run()
    {
        var migrationTimeCounter = new Stopwatch();
        migrationTimeCounter.Start();

        Tracer.TraceLine(" - Migration start...");

        try
        {
            foreach (var pipeline in Pipelines)
            {
                pipeline.Initialize(Tracer);
            }

            foreach (var pipeline in Pipelines)
            {
                pipeline.Run();
            }
        }
      
        catch (DataMigrationException e)
        {
            Tracer.TraceMigrationException("Error occured while pipeline processing", e);
            if (BreakOnError)
                throw;
        }
        catch (Exception e)
        {
            Tracer.TraceLine(e.ToString());
            if (BreakOnError)
                throw;;
        }
                
        migrationTimeCounter.Stop();
        Tracer.TraceLine($"\n - Migration end {migrationTimeCounter.Elapsed.TotalMinutes} mins");
    }
}