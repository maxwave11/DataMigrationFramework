using System;
using System.Diagnostics;
using System.Linq;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;
using DataMigration.Pipeline.Trace;

namespace DataMigration
{
    public interface IMigrationLogger
    {
        void Log(ConsoleColor color, string text);
    }

    public class Migrator : IMigrator
    {
        internal MigrationTracer Tracer { get; }

        public static Migrator Current  {get; private set; }
        public bool BreakOnError { get; }

        private readonly MapConfig _mapConfig;
        private readonly IMigrationLogger _logger;

        public Migrator(MapConfig mapConfig, IMigrationLogger logger)
        {
            if (mapConfig == null)
                throw new ArgumentNullException(nameof(mapConfig));

            _mapConfig = mapConfig;
            _logger = logger;
            Current = this;
            Tracer = new MigrationTracer();
            Tracer.Trace += Migrator_Trace;
        }

        private void Migrator_Trace(object sender, TraceMessage e)
        {
            _logger.Log(e.Color, e.Text);
        }

        public void Run()
        {
            var migrationTimeCounter = new Stopwatch();
            migrationTimeCounter.Start();

            Tracer.TraceLine(" - Migration start...");

            try
            {
                InitializeVariables();
                RunPipelines();
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
                    throw;
            }
                
            migrationTimeCounter.Stop();
            
            Tracer.TraceLine($"\n - Migration end {migrationTimeCounter.Elapsed.TotalMinutes} mins");
        }

        private void InitializeVariables() 
        {
            if (_mapConfig.Variables == null)
                return;

            //Calculate variable values from appropriate YAML expressions
            foreach (string varName in _mapConfig.Variables.Keys)
            {
                if (_mapConfig.Variables[varName] is CommandBase command)
                {
                    var ctx = new ValueTransitContext(null, null);
                    ctx.Execute(command);
                    _mapConfig.Variables[varName] = ctx.TransitValue;
                }
            }
        }

        private void RunPipelines() 
        {
            foreach (var pipeline in _mapConfig.Pipeline)
            {
                pipeline.Run();
            }
        }
    }
}


