using System;
using System.Collections.Generic;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Pipeline.Trace;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline
{
    public class ValueTransitContext
    {
        public IValuesObject Source { get; }
        public IValuesObject Target { get; }

        public TransitionFlow Flow { get; set; }

        public object TransitValue { get; private set; }
        
        public DataPipeline DataPipeline { get; set; }
        
        public CommandBase CurrentNode { get; set; }

        public readonly List<TraceMessage> TraceEntries = new List<TraceMessage>();

        public bool Trace { get; set; }

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceMessage(msg, color));
        }

        public ValueTransitContext(IValuesObject source, IValuesObject target, object transitValue)
        {
            Source = source;
            Target = target;
            TransitValue = transitValue;
        }

        public void SetCurrentValue(object value)
        {
            TransitValue = value;
            var valueType = TransitValue?.GetType().Name.Truncate(30);
            string message = $"==> ({valueType}){TransitValue?.ToString().Truncate(80)}";
            Migrator.Current.Tracer.TraceLine(message, this, ConsoleColor.DarkGray);
        }
        
        public void ResetCurrentValue()
        {
            TransitValue = null;
        }
    }
}