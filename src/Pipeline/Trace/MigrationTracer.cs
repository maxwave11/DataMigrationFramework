using System;
using System.Collections.Generic;
using System.Linq;
using DataMigration.Enums;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Trace
{
    public sealed class MigrationTracer : IMigrationTracer
    {
        /// <summary>
        /// Use this event to trace migration process
        /// </summary>
        public event EventHandler<TraceMessage> Trace = delegate { };

        /// <summary>
        /// Event fires each time when any value transition started. By use this event
        /// you can control (for example stop/pause) migration flow.
        /// </summary>
        public event EventHandler TransitValueStarted;

        private const string IndentUnit = "    ";

        private int _identLevel = 0;

        public void TraceLine(string message, ValueTransitContext ctx = null, ConsoleColor color = ConsoleColor.White)
        {
            message = FormatMessage(message);

            ctx?.AddTraceEntry(message, color);

            if (ctx == null || ctx.Trace)
                SendTraceMessage(message, color);
        }

        public void TraceMigrationException(string message, DataMigrationException ex)
        {
            string msg = FormatMessage(message);

            var entriesBeforeError = ex.Context.TraceEntries.ToList();
            ex.Context.Trace = true;
            TraceLine(msg, ex.Context, ConsoleColor.Red);

            TraceLine("\nException:", ex.Context, ConsoleColor.Red);
            TraceLine($"\n{ex.InnerException.ToString()}", ex.Context, ConsoleColor.Red);

            TraceLine("\nTRACE:", ex.Context, ConsoleColor.Red);
            entriesBeforeError.ForEach(i => Trace.Invoke(this, i));

            TraceLine("\nSRC:", ex.Context, ConsoleColor.Red);
            TraceLine("\n" + ex.Context.Source?.GetInfo(), ex.Context, ConsoleColor.Red);

            TraceLine("\nTARGET:", ex.Context, ConsoleColor.Red);
            TraceLine("\n" + ex.Context.Target?.GetInfo(), ex.Context, ConsoleColor.Red);
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
            var indentString = "";
            for (int i = 0; i < _identLevel; i++)
                indentString += IndentUnit;

            return '\n' + msg.Split('\n').Select(i => indentString + i).Join("\n");
        }

        public void Indent()
        {
            _identLevel++;
        }

        public void IndentBack()
        {
            _identLevel--;
        }

        public void SaveLogs()
        {
            //using (var writer = new StreamWriter("events.csv"))
            //using (var csv = new CsvWriter(writer, new CsvConfiguration() { Delimiter = ";" }))
            //{
            //    csv.WriteRecords(_migrationEvents);
            //}
        }
    }
}