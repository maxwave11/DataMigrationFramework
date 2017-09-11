using System;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using ExpressionCompiler = XQ.DataMigration.Mapping.Expressions.ExpressionCompiler;

namespace XQ.DataMigration.Mapping
{
    public class Migrator
    {
        public MigrationTracer Tracer { get; }

        internal static Migrator Current => _current;
        internal MapAction Action { get; private set; }
        public ExpressionCompiler ExpressionCompiler { get; } = new ExpressionCompiler();

        private static Migrator _current;
        private readonly MapConfig.MapConfig _mapConfig;

        public Migrator(MapConfig.MapConfig mapConfig)
        {
            _mapConfig = mapConfig;
            _current = this;
            Tracer = new MigrationTracer();
        }

        public void Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Tracer.TraceText("====== Migration start ======");

            foreach (MapAction action in this._mapConfig.MapActions.Where(i => i.DoMapping))
            {
                using (action)
                {
                    action.Initialize();
                    Action = action;
                    MapAction(action);
                }
            }

            stopwatch.Stop();
            Tracer.TraceText($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }

        private void MapAction(MapAction action)
        {
            var transGroup = action.MapConfig.TransitionGroups.FirstOrDefault();
            var ctx = new ValueTransitContext(null,null,null,null);
            transGroup?.TransitInternal(ctx);
        }
    }
}


