using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using CsvHelper;
using CsvHelper.Configuration;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Trace
{
    public class MigrationEventTraceEntry
    {
        public uint RowNumber { get; set; }
        public string DataSetName { get; set; }

        public MigrationEvent EventType { get; }

        public string Query { get; set; }

        public string ObjectKey { get; set; }

        public string Message { get; }

        public MigrationEventTraceEntry(MigrationEvent eventType, ValueTransitContext ctx, string message)
        {
            EventType = eventType;
            Message = message;
            ObjectKey = ctx.Source.Key;
            DataSetName = ctx.DataPipeline?.Name;
            RowNumber = ctx.Source.RowNumber;
            Query = ctx.DataPipeline?.Source.ToString();
        }

    }

    public sealed class MigrationTracer
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

        /// <summary>
        /// Event fires each time when any unhandled error occured while migration process
        /// </summary>
        public event EventHandler<TransitErrorEventArgs> OnValueTransitError;

        private const string _indentUnit = "    ";

        private int _identLevel = 0;

        private List<MigrationEventTraceEntry> _migrationEvents = new List<MigrationEventTraceEntry>();

        public void TraceLine(string message, ValueTransitContext ctx = null, ConsoleColor color = ConsoleColor.White)
        {
            message = FormatMessage(message);

            ctx?.AddTraceEntry(message, color);

            if (ctx == null || ctx.Trace)
                SendTraceMessage(message, color);
        }

        public void TraceEvent(MigrationEvent eventType, ValueTransitContext ctx, string message)
        {
            message = FormatMessage(message);

            ctx.AddTraceEntry(message, ConsoleColor.Yellow);

            _migrationEvents.Add(new MigrationEventTraceEntry(eventType, ctx, message));

            SendTraceMessage(message, ConsoleColor.Yellow);
        }

        public void TraceError(string message, string errorMessage, ValueTransitContext ctx)
        {
            string msg = FormatMessage(message);
            TraceLine(msg, ctx, ConsoleColor.Red);

            TraceLine("\nERROR ====================", ctx, ConsoleColor.Red);
            TraceLine($"\n{errorMessage}", ctx, ConsoleColor.Red);
            TraceLine("\nTRACE:", ctx, ConsoleColor.Red);
            ctx.TraceEntries.ForEach(i => Trace.Invoke(this, i));

            TraceLine("\nSRC:", ctx, ConsoleColor.Red);
            TraceLine("\n" + ctx.Source?.GetInfo(), ctx, ConsoleColor.Red);

            TraceLine("\nTARGET:", ctx, ConsoleColor.Red);
            TraceLine("\n" + ctx.Target?.GetInfo(), ctx, ConsoleColor.Red);

            var t = this;
            OnValueTransitError?.Invoke(this, new TransitErrorEventArgs(ctx));
        }

        private void SendTraceMessage(string msg, ConsoleColor color)
        {
            Trace.Invoke(this, new TraceMessage(msg, color));
        }

        private string FormatMessage(string msg)
        {
            var indentString = "";
            for (int i = 0; i < _identLevel; i++)
                indentString += _indentUnit;

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
            using (var writer = new StreamWriter("events.csv"))
            using (var csv = new CsvWriter(writer, new CsvConfiguration() { Delimiter = ";" }))
            {
                csv.WriteRecords(_migrationEvents);
            }
        }
    }
}