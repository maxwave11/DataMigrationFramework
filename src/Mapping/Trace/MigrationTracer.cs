using System;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfiguration;
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

        internal const string IndentUnit = "   ";

        internal int _identLevel = 0;

        public void TraceLine(string message)
        {
            message = FormatMessage(message);
            Trace?.Invoke(this, new TraceMessage(message, ConsoleColor.White));
        }

        public void TraceLine(string message, TransitionNode node, ValueTransitContext ctx)
        {
            if (!string.IsNullOrEmpty(message))
                message = FormatMessage(message);

            // if (node.HasParentOfType<KeyTransition>() && node.ActualTrace == false)
            // {
            //     //don't add KeyTransition's TraceEntries to log if it's disabled
            // }
            // else
            {
                ctx?.AddTraceEntry(message, node.Color);
            }

            var doTrace = node.ActualTrace;

            //switch (node.ActualTrace)
            //{
            //    case TraceLevel.None:
            //        doTrace = false;
            //        break;
            //    case TraceLevel.ObjectSet:
            //        if (node.HasParentOfType<TransitDataCommand>() && !(node is TransitDataCommand))
            //            doTrace = false;
            //        break;
            //    case TraceLevel.Object:
            //        if (node.HasParentOfType<ObjectTransition>() && !(node is ObjectTransition))
            //            doTrace = false;
            //        break;
            //    default:
            //        break;
            //}
            
            if (doTrace)
                Trace?.Invoke(this, new TraceMessage(message, node.Color));
        }

        public void TraceWarning(string message)
        {
            if (!string.IsNullOrEmpty(message))
                message = FormatMessage("WARNING:" + message);


            //ctx.AddTraceEntry(message, ConsoleColor.Yellow);

            Trace?.Invoke(this, new TraceMessage(message, ConsoleColor.Yellow));
        }

        public void TraceError(string message, TransitionNode node, ValueTransitContext ctx)
        {
            var msg = FormatMessage(message);
            ctx.AddTraceEntry(msg, ConsoleColor.Yellow);

            Trace?.Invoke(this, new TraceMessage(msg, ConsoleColor.Red));

            var args = new TransitErrorEventArgs(node, ctx);
            OnValueTransitError?.Invoke(node, args);
        }

        private string FormatMessage(string msg)
        {
            var indent = "";
            for (int i = 0; i < _identLevel; i++)
                indent += IndentUnit;
          
            return '\n' + msg.Split('\n').Select(i => indent + i).Join("\n");
        }

        // private ObjectTransition FindObjectTransition(TransitionNode transitionNode)
        // {
        //     if (transitionNode == null)
        //         return null;
        //
        //     return  (transitionNode as ObjectTransition) ?? FindObjectTransition(transitionNode.Parent);
        // }

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