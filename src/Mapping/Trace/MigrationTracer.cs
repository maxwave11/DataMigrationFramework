using System;
using System.Linq;
using XQ.DataMigration.Data;
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
            AddTraceEntry(node, msg, color);

            if (node.ActualTrace == TraceMode.True || (node.ActualTrace == TraceMode.Auto && node is ObjectTransition))
                Trace?.Invoke(this, new TraceMessage(msg, color, node));
        }

        public void TraceUserMessage(string message, TransitionNode node)
        {
            AddTraceEntry(node, message, node.Color);
            Trace?.Invoke(this, new TraceMessage(message, node.Color, node));
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

        public void TraceTransitionNodeStart(TransitionNode transitionNode, ValueTransitContext ctx)
        {
            var traceMsg =
                $"> {transitionNode.ToString()}\n    Input: ({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";

            TraceText(traceMsg, transitionNode);
        }

        public void TraceTransitionNodeEnd(TransitionNode transitionNode, ValueTransitContext ctx)
        {
            var traceMsg = $"< =({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            TraceText(traceMsg, transitionNode);
        }

        public void TraceSkipObject(string text, TransitionNode node, IValuesObject sourceObject)
        {
            TraceText(text, node, ConsoleColor.Yellow);
            OnObjectSkipped?.Invoke(this, sourceObject);
        }

        public TransitContinuation TraceError(string message, TransitionNode valueTransition, ValueTransitContext ctx)
        {
            TraceText(message, valueTransition, ConsoleColor.Red);
            var args = new TransitErrorEventArgs(valueTransition, ctx);
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
            var objectTransition = FindObjectTransition(node);
            objectTransition.AddTraceEntry(message, color);
        }

        private ObjectTransition FindObjectTransition(TransitionNode transitionNode)
        {
            return  (transitionNode as ObjectTransition) ?? FindObjectTransition(transitionNode.Parent);
        }
    }
}