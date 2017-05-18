using System;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Trace
{
    public sealed class MigrationTracer
    {
        /// <summary>
        /// Use this event to trace migration process
        /// </summary>
        public event EventHandler<TraceMessage> Trace;

        /// <summary>
        /// Event fires each time when any value transition started. By use this event
        /// you can control (for example stop/pause) migration flow.
        /// </summary>
        public event EventHandler TransitValueStarted;

        /// <summary>
        /// Event fires each time when any unhandled error occured while migration process
        /// </summary>
        public event EventHandler<TransitErrorEventArgs> OnValueTransitError;

        /// <summary>
        /// Event fires each time when any unhandled error occured while migration process
        /// </summary>
        public event EventHandler<IValuesObject> OnObjectSkipped;

        public void TraceText(string message)
        {
            Trace?.Invoke(this, new TraceMessage(message, ConsoleColor.White, null));
        }

        public void TraceText(string message, TransitionNode node)
        {
            TraceText(message, node, node.Color);
        }

        public void TraceText(string message, TransitionNode node, ConsoleColor color)
        {
            var msg =  message.Split('\n').Select(i => GetIndent(node) + i).Join("\n");
            AddTraceEntryToObjectTransition(node, msg, color);

            if (node.ActualTrace == TraceMode.True || (node.ActualTrace == TraceMode.Auto && (node is ObjectTransition || node is ObjectSetTransition) ))
                Trace?.Invoke(this, new TraceMessage(msg, color, node));
        }

        public void TraceWarning(string message, TransitionNode node)
        {
            var msg = message.Split('\n').Select(i => GetIndent(node) + i).Join("\n");
            AddTraceEntryToObjectTransition(node, msg, ConsoleColor.Yellow);

            Trace?.Invoke(this, new TraceMessage(msg, ConsoleColor.Yellow, node));
        }

        public void TraceObjectSetTransitionStart(ObjectSetTransition transition)
        {
            TraceText($">>>Transitting all objects from  source DataSet '{transition.Name}'", transition, ConsoleColor.DarkYellow);
        }

        public void TraceObjectSetTransitionEnd(ObjectSetTransition transition)
        {
            TraceText($"<<< Objects from source DataSet '{transition.Name}' are transitted'\n", transition, ConsoleColor.DarkYellow);
        }

        public void TraceSkipObject(string text, TransitionNode node, IValuesObject sourceObject)
        {
            TraceText(text, node, ConsoleColor.Yellow);
            OnObjectSkipped?.Invoke(this, sourceObject);
        }

        public TransitContinuation TraceError(string message, TransitionNode node, ValueTransitContext ctx)
        {
            var msg = message.Split('\n').Select(i => GetIndent(node) + i).Join("\n");
            AddTraceEntryToObjectTransition(node, msg, ConsoleColor.Yellow);

            Trace?.Invoke(this, new TraceMessage(msg, ConsoleColor.Red, node));

            var args = new TransitErrorEventArgs(node, ctx);
            OnValueTransitError?.Invoke(node, args);
            return args.Continue ? TransitContinuation.Continue : TransitContinuation.Stop;
        }

        protected string GetIndent(TransitionNode node)
        {
            var _indent = "";
            TransitionNode nextParent = node.Parent;
            while (nextParent != null)
            {
                nextParent = nextParent.Parent;
                _indent += "  ";
            }

            return _indent;
        }

        private void AddTraceEntryToObjectTransition(TransitionNode node, string message, ConsoleColor color)
        {
            var objectTransition = FindObjectTransition(node);
            objectTransition?.AddTraceEntry(message, color);
        }

        private ObjectTransition FindObjectTransition(TransitionNode transitionNode)
        {
            if (transitionNode == null)
                return null;

            return  (transitionNode as ObjectTransition) ?? FindObjectTransition(transitionNode.Parent);
        }
    }
}