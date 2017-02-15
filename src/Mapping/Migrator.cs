﻿using System;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using ExpressionCompiler = XQ.DataMigration.Mapping.Expressions.ExpressionCompiler;

namespace XQ.DataMigration.Mapping
{
    public class Migrator
    {
        /// <summary>
        /// Event fires each time when any value transition started. By use this event
        /// you can control (for example stop/pause) migration flow.
        /// </summary>
        public event EventHandler TransitValueStarted;

        /// <summary>
        /// Event fires each time when any unhandled error occured while migration process
        /// </summary>
        public event EventHandler<ValueTransitErrorEventArgs> OnValueTransitError;

        /// <summary>
        /// Use this event to trace migration process
        /// </summary>
        public event EventHandler<string> Log;

        internal static Migrator Current => _current;
        internal MapAction Action { get; private set; }
        internal ExpressionCompiler ExpressionCompiler { get; } = new ExpressionCompiler();

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

            TransitLogger.Log("====== Migration start ======");

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
            TransitLogger.Log($"====== END {stopwatch.Elapsed.TotalMinutes} mins ======");
        }

        private void MapAction(MapAction action)
        {
            TransitLogger.Log($"====== Action: {action.MapDataBaseName} ========");
            var transGroup = action.MapConfig.TransitionGroups.FirstOrDefault();
            transGroup?.Run();
        }

        internal void RaiseTransitValueStarted(ValueTransitionBase valueTransition)
        {
            TransitValueStarted?.Invoke(valueTransition, null);
        }

        internal TransitContinuation InvokeOnTransitError(ValueTransitionBase valueTransition, ValueTransitContext ctx)
        {
            var args = new ValueTransitErrorEventArgs(valueTransition, ctx);
            OnValueTransitError?.Invoke(valueTransition, args);
            return args.Continue ? TransitContinuation.Continue : TransitContinuation.Stop;
        }

        public void InvokeLog(string logMessage)
        {
            Log?.Invoke(this, logMessage);
        }

    }
}


