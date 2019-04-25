using System;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using ExpressionCompiler = XQ.DataMigration.Mapping.Expressions.ExpressionCompiler;

namespace XQ.DataMigration.Mapping
{
    public class Migrator
    {
        public MigrationTracer Tracer { get; }

        internal static Migrator Current  {get; private set; }
        public ExpressionCompiler ExpressionCompiler { get; } = new ExpressionCompiler();

        internal MapConfig.MapConfig MapConfig { get; private set; }

        public Migrator(MapConfig.MapConfig mapConfig)
        {
            MapConfig = mapConfig;
            Current = this;
            Tracer = new MigrationTracer();
        }

        public void Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Tracer.TraceLine("====== Migration start ======");

            var ctx = new ValueTransitContext(null, null, null, null);
            MapConfig.ChildTransitions?.ForEach(i => i.TransitCore(ctx));

            stopwatch.Stop();
            Tracer.TraceLine($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }
    }
}


