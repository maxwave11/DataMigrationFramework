using System;
using System.Linq;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Trace
{
    public sealed class MigrationTracer
    {
        /// <summary>
        /// Use this event to trace migration process
        /// </summary>
        public event EventHandler<TraceMessage> Trace = delegate { };

        public bool TraceEnabled { get; set; } = false;

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

        public void TraceLine(string message, ConsoleColor color = ConsoleColor.White, ValueTransitContext ctx = null)
        {
            if (!string.IsNullOrEmpty(message))
                message = FormatMessage(message);

            ctx?.AddTraceEntry(message, color);

            if (ctx == null || ctx.Trace)
                Trace.Invoke(this, new TraceMessage(message, color));
        }

        public void TraceWarning(string message, ValueTransitContext ctx)
        {
            if (!string.IsNullOrEmpty(message))
                message = FormatMessage("WARNING:" + message);

            ctx.AddTraceEntry(message, ConsoleColor.Yellow);

            Trace.Invoke(this, new TraceMessage(message, ConsoleColor.Yellow));
        }

        public void TraceError(string message, ValueTransitContext ctx)
        {
            var msg = FormatMessage(message);
            ctx.AddTraceEntry(msg, ConsoleColor.Yellow);

            Trace.Invoke(this, new TraceMessage(msg, ConsoleColor.Red));

            var args = new TransitErrorEventArgs(ctx);
            OnValueTransitError?.Invoke(ctx, args);
        }

        private string FormatMessage(string msg)
        {
            var indent = "";
            for (int i = 0; i < _identLevel; i++)
                indent += IndentUnit;
          
            return '\n' + msg.Split('\n').Select(i => indent + i).Join("\n");
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