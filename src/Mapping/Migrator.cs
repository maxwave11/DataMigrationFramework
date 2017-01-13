using System;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using ExpressionCompiler = XQ.DataMigration.Mapping.Expressions.ExpressionCompiler;

namespace XQ.DataMigration.Mapping
{
    public class ValueTransitErrorEventArgs
    {
        public ValueTransitErrorEventArgs(ValueTransitionBase valueTransition, ValueTransitContext context)
        {
            ValueTransition = valueTransition;
            Context = context;
        }

        public ValueTransitionBase ValueTransition { get; private set; }
        public ValueTransitContext Context { get; private set; }
        public TransitContinuation Continuation { get; set; } = TransitContinuation.Continue;
    }

    public class Migrator
    {
        public event EventHandler TransitValueStarted;
        public event EventHandler<ValueTransitErrorEventArgs> OnValueTransitError;
        public static Migrator Current => _current;
        public MapAction Action { get; private set; }
        public ExpressionCompiler ExpressionCompiler { get; } = new ExpressionCompiler();

        private static Migrator _current;
        private readonly MapConfig.MapConfig _mapConfig;

        public Migrator(MapConfig.MapConfig mapConfig)
        {
            _mapConfig = mapConfig;
            _current = this;
        }

        public void Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            TransitLogger.LogInfo("====== Migration start ======");

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
            TransitLogger.LogInfo($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }

        private void MapAction(MapAction action)
        {
            TransitLogger.LogInfo($"====== Action: {action.MapDataBaseName} ========");
            var transGroup = action.MapConfig.TransitionGroups.FirstOrDefault();
            transGroup?.Run();
        }

        public void RaiseTransitValueStarted(ValueTransitionBase valueTransition)
        {
            TransitValueStarted?.Invoke(valueTransition, null);
        }

        public TransitContinuation RaiseOnTransitError(ValueTransitionBase valueTransition, ValueTransitContext ctx)
        {
            var args = new ValueTransitErrorEventArgs(valueTransition, ctx);
            OnValueTransitError?.Invoke(valueTransition, args);
            return args.Continuation;
        }
    }
}


