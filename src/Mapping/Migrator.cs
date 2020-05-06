using System;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using ExpressionCompiler = XQ.DataMigration.Mapping.Expressions.ExpressionCompiler;

namespace XQ.DataMigration.Mapping
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

            var ctx = new ValueTransitContext(null, null, null);

            try
            {
                _mapConfig.Pipeline.Where(i=>i.Enabled).ToList().ForEach(i=>i.Run());
            }
            catch (Exception e)
            {
                Tracer.TraceLine(e.Message);
            }
                
            stopwatch.Stop();
            Tracer.TraceLine($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }
    }
}


