using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Logic
{
    public class ValueTransitContext
    {
        public IValuesObject Source { get; set; }
        public IValuesObject Target { get; set; }

        //enables trace from any place of configuration (change it by expression). Should reset after value transition stop
        public bool Trace { get; set; }
        
        public FlowControl Flow { get; set; }

        public object TransitValue { get; private set; }
        
        public TransitDataCommand TransitDataCommand { get; set; }
        
        public TransitionNode CurrentNode { get; set; }

        public readonly Dictionary<string, object> _valuesHistory = new Dictionary<string, object>();

        public readonly List<TraceEntry> TraceEntries = new List<TraceEntry>();

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

        public void SetCurrentValue(string transitionName, object value)
        {
            TransitValue = value;
            if (transitionName.IsEmpty())
                return;

            _valuesHistory[transitionName] = value;
        }

        public object GetHistoricValue(string transitionName)
        {
            if (!_valuesHistory.ContainsKey(transitionName))
                throw new Exception($"There are no value from transition { transitionName }");
            return _valuesHistory[transitionName];
        }
    }
}