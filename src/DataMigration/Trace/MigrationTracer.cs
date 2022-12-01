using System;
using System.Linq;
using DataMigration.Enums;
using DataMigration.Pipeline;
using DataMigration.Utils;

namespace DataMigration.Trace
{
    public sealed class MigrationTracer : IMigrationTracer
    {
        private readonly TraceMode _traceMode;

        /// <summary>
        /// Use this event to trace migration process
        /// </summary>
        public event EventHandler<TraceMessage> Trace = delegate { };

        private const string IndentUnit = "    ";
        private int _identLevel = 0;
        
        public MigrationTracer(TraceMode traceMode)
        {
            _traceMode = traceMode;
        }

        public void TraceLine(string message, ValueTransitContext ctx = null, TraceMode level = TraceMode.Pipeline, ConsoleColor color = ConsoleColor.White)
        {
            ctx?.AddTraceEntry(message, color);
            message = FormatMessage(message);

            if ((_traceMode & level) == level)
                SendTraceMessage(message, color);
        }

        public void TraceMigrationException(string message, DataMigrationException ex)
        {
            var entriesBeforeError = ex.Context.TraceEntries.ToList();
            TraceLine(message, ex.Context, color: ConsoleColor.Red);

            TraceLine("\nEXCEPTION:", ex.Context);
            Indent();
            TraceLine($"\n{ex.InnerException}", ex.Context, color: ConsoleColor.Red);
            IndentBack();
            
            TraceLine("\nTRACE:\n", ex.Context);
            Indent();
            entriesBeforeError.ForEach(i => TraceLine(i.Text, null, color: i.Color));
            IndentBack();

            TraceLine("\nSRC:", ex.Context);
            Indent();
            TraceLine("\n" + ex.Context.Source, ex.Context, color: ConsoleColor.DarkGray);
            IndentBack();

            TraceLine("\nTARGET:", ex.Context);
            Indent();
            TraceLine("\n" + ex.Context.Target, ex.Context, color: ConsoleColor.DarkGray);
            IndentBack();
        }

        /// <summary>
        /// Send trace massage to external subscriber
        /// </summary>
        private void SendTraceMessage(string message, ConsoleColor color)
        {
            Trace.Invoke(this, new TraceMessage(message, color));
        }

        private string FormatMessage(string msg)
        {
            return '\n' + msg.Split('\n').Select(i => GetIdentString() + i).Join("\n");
        }

        private string GetIdentString()
        {
            return Enumerable.Repeat(IndentUnit, _identLevel).Join("");
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