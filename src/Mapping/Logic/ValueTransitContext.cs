using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Logic
{
    public class ValueTransitContext
    {
        public IValuesObject Source { get; }
        public IValuesObject Target { get; }

        public TransitionFlow Flow { get; set; }

        public object TransitValue { get; private set; }
        
        public TransitDataCommand TransitDataCommand { get; set; }
        
        public TransitionNode CurrentNode { get; set; }

        public readonly List<TraceEntry> TraceEntries = new List<TraceEntry>();

        public bool Trace { get; set; }

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceEntry() { Mesage = msg, Color = color });
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
        }
    }
}