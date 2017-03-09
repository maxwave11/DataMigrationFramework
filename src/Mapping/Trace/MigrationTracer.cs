using System;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
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
        public event EventHandler<ValueTransitErrorEventArgs> OnValueTransitError;

        public void TraceText(string message)
        {
            Trace?.Invoke(this, new TraceMessage(message, ConsoleColor.White));
        }

        public void TraceText(string message, TransitionNode node)
        {
            TraceText(message, node, node.Color);
        }

        public void TraceText(string message, TransitionNode node, ConsoleColor color)
        {
            var msg = GetIndent(node) + message;
            AddTraceEntry(node, msg, color);
            Trace?.Invoke(this, new TraceMessage(msg, color));
        }

        public void TraceUserMessage(string message, TransitionNode node)
        {
            TraceText(message, node);
        }

        public void TraceObjectTransitionStart(ObjectTransition objectTransition, string objectKey)
        {
            TraceText($"(Start object transition ({objectTransition.Name}) [{ objectKey }]", objectTransition);
        }

        public void TraceObjectTransitionEnd(ObjectTransition objectTransition)
        {
            TraceText("(End object transition)", objectTransition);
        }

        public void TraceObjectSetTransitionStart(ObjectTransition transition)
        {
            TraceText($">>>Transitting all objects from  source DataSet '{transition.Name}' to target DataSet'{transition.TargetDataSetId}'", transition, ConsoleColor.DarkYellow);
        }

        public void TraceObjectSetTransitionEnd(ObjectTransition transition)
        {
            TraceText($"<<< Objects from source DataSet '{transition.Name}' are transitted to target DataSet '{transition.TargetDataSetId}'\n", transition, ConsoleColor.DarkYellow);
        }

        public void TraceValueTransitionStart(ValueTransitionBase valueTransition, ValueTransitContext ctx)
        {
            if (valueTransition.ActualTrace != TraceMode.True)
                return;

            var traceMsg =
                $"> {valueTransition.ToString()}\n\tInput: ({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";

            TraceText(traceMsg, valueTransition);
        }

        public void TraceValueTransitionEnd(ValueTransitionBase valueTransition, ValueTransitContext ctx)
        {
            if (valueTransition.ActualTrace != TraceMode.True)
                return;

            var traceMsg = $"< =({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            TraceText(traceMsg, valueTransition);
        }

        public void TraceSkipObject(string text, TransitionNode node)
        {
            TraceText(text, node, ConsoleColor.Yellow);
        }

        public TransitContinuation TraceError(string message, ValueTransitionBase valueTransition, ValueTransitContext ctx)
        {
            TraceText(message, valueTransition, ConsoleColor.Red);
            var args = new ValueTransitErrorEventArgs(valueTransition, ctx);
            OnValueTransitError?.Invoke(valueTransition, args);
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

        private void AddTraceEntry(TransitionNode node, string message, ConsoleColor color)
        {
            if (node is ValueTransitionBase)
                ((ValueTransitionBase)node).ObjectTransition.AddTraceEntry(GetIndent(node) + message, color);

            if (node is ObjectTransition)
                ((ObjectTransition)node).AddTraceEntry(GetIndent(node) + message, color);
        }
    }
}