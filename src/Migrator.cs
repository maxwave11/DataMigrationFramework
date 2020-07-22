using System;
using System.CodeDom;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Pipeline.Trace;

namespace XQ.DataMigration
{
    public class Migrator
    {
        public MigrationTracer Tracer { get; }

        public static Migrator Current  {get; private set; }
        public bool ThrowExeptionOnError { get; }

        private MapConfig _mapConfig;

        public Migrator(MapConfig mapConfig, bool throwExeptionOnError = false)
        {
            _mapConfig = mapConfig;
            ThrowExeptionOnError = throwExeptionOnError;
            Current = this;
            Tracer = new MigrationTracer();
        }

        public void Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Tracer.TraceLine("====== Migration start ======");

            try
            {
                foreach (string name in _mapConfig.Variables.Keys.ToList())
                {
                    if (_mapConfig.Variables[name] is CommandBase command)
                    {
                        var ctx = new ValueTransitContext(null, null);
                        ctx.Execute(command);
                        _mapConfig.Variables[name] = ctx.TransitValue;
                    }
                }

                foreach (var pipeline in _mapConfig.Pipeline.Where(i => i.Enabled)) 
                {
                    pipeline.Run();
                }
            }
            catch (DataMigrationException e)
            {
                Tracer.TraceMigrationException("Error occured while pipeline processing", e);
                if (ThrowExeptionOnError)
                    throw;
            }
            catch (Exception e)
            {
                Tracer.TraceLine(e.ToString());
                if (ThrowExeptionOnError)
                    throw;
            }
                
            stopwatch.Stop();
            Tracer.SaveLogs();
            Tracer.TraceLine($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }
    }
}


