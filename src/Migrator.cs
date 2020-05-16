using System;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Pipeline.Trace;

namespace XQ.DataMigration
{
    public class Migrator
    {
        public MigrationTracer Tracer { get; }

        public static Migrator Current  {get; private set; }

        private MapConfig _mapConfig;

        public Migrator(MapConfig mapConfig)
        {
            _mapConfig = mapConfig;
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
                foreach (var node in _mapConfig.Pipeline.Where(i => i.Enabled)) 
                {
                    node.Run();
                }
            }
            catch (Exception e)
            {
                Tracer.TraceLine(e.ToString());
            }
                
            stopwatch.Stop();
            Tracer.TraceLine($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }
    }
}


