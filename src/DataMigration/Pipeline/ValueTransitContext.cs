using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.Interfaces;
using DataMigration.Enums;
using DataMigration.Trace;
using DataMigration.Utils;

namespace DataMigration.Pipeline
{
    public class ValueTransitContext
    {
        public object Source { get; }
        public object  Target { get; set; }

        public PipelineFlowControl FlowControl { get; set; }

        public object TransitValue { get; private set; }
        
       // public DataPipeline DataPipeline { get; set; }
        
        public readonly List<TraceMessage> TraceEntries = new List<TraceMessage>();
        
        // For debug purposes
        public string _traceEntries => TraceEntries.Select(i => i.Text).Join(" ");

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceMessage(msg, color));
        }

        public ValueTransitContext(object source, object transitValue)
        {
            Source = source;
            TransitValue = transitValue;
        }

        public void SetCurrentValue(object value)
        {
            TransitValue = value;
        }
        
        public void ResetCurrentValue()
        {
            TransitValue = null;
        }
    }
}