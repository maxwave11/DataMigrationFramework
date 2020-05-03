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

        internal const string IndentUnit = "\t";

        internal int _identLevel = 0;

        public void TraceLine(string message)
        {
            Trace?.Invoke(this, new TraceMessage('\n' + message, ConsoleColor.White, null));
        }

        public void TraceLine(string message, TransitionNode node)
        {
            TraceLine(message, node, node.Color);
        }

        public void TraceLine(string message, TransitionNode node, ConsoleColor color)
        {
            if (!string.IsNullOrEmpty(message))
                message = FormatMessage(message, node);

            if (node.HasParentOfType<KeyTransition>() && node.ActualTrace == TraceLevel.None)
            {
                //don't add KeyTransition's TraceEntries to log if it's disabled
            }
            else
            {
                AddTraceEntryToObjectTransition(node, message, color);
            }

            var doTrace = true;

            switch (node.ActualTrace)
            {
                case TraceLevel.None:
                    doTrace = false;
                    break;
                case TraceLevel.ObjectSet:
                    if (node.HasParentOfType<TransitDataCommand>() && !(node is TransitDataCommand))
                        doTrace = false;
                    break;
                case TraceLevel.Object:
                    if (node.HasParentOfType<ObjectTransition>() && !(node is ObjectTransition))
                        doTrace = false;
                    break;
                default:
                    break;
            }
            
            if (doTrace)
                Trace?.Invoke(this, new TraceMessage(message, color, node));
        }

        public void TraceWarning(string message, TransitionNode node)
        {
            if (!node.TraceWarnings)
                return;
            
            if (!string.IsNullOrEmpty(message))
                message = FormatMessage("WARNING:" + message, node);

            AddTraceEntryToObjectTransition(node, message, ConsoleColor.Yellow);

            Trace?.Invoke(this, new TraceMessage(message, ConsoleColor.Yellow, node));
        }

        public void TraceSkipObject(string text, TransitionNode node, IValuesObject sourceObject)
        {
            TraceLine(text, node, ConsoleColor.Yellow);
            OnObjectSkipped?.Invoke(this, sourceObject);
        }

        public TransitContinuation TraceError(string message, TransitionNode node, ValueTransitContext ctx)
        {
            var msg = FormatMessage(message, node);
            AddTraceEntryToObjectTransition(node, msg, ConsoleColor.Yellow);

            Trace?.Invoke(this, new TraceMessage(msg, ConsoleColor.Red, node));

            var args = new TransitErrorEventArgs(node, ctx);
            OnValueTransitError?.Invoke(node, args);
            return args.Continue ? TransitContinuation.Continue : TransitContinuation.Stop;
        }

        private string FormatMessage(string msg, TransitionNode node)
        {
            var indent = "";
            // TransitionNode nextParent = node.Parent;

            for (int i = 0; i < _identLevel; i++)
            {
                indent += IndentUnit;
            }
            // while (nextParent != null)
            // {
            //     nextParent = nextParent.Parent;
            //     indent += IndentUnit;
            // }

            return '\n' + msg.Split('\n').Select(i => indent + i).Join("\n");
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

        public void Indent()
        {
            _identLevel++;
        }
        
        public void IndentBack()
        {
            _identLevel--;
        }
    }
}