using System;
using System.Collections.Generic;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Logic
{
    public class TraceEntry
    {
        public string Mesage { get; set; }
        public ConsoleColor Color { get; set; }
    }
    public class ValueTransitContext
    {
        public IValuesObject Source { get; }
        public IValuesObject Target { get; }
        public object TransitValue { get; private set; }
        public ObjectTransition ObjectTransition { get; }
        public Type ValueType { get; }
        public Dictionary<string, object> _valuesHistory = new Dictionary<string, object>();
        public List<TraceEntry> TraceEntries = new List<TraceEntry>();
        public ValueTransitContext(IValuesObject source, IValuesObject target, object transitValue, ObjectTransition objectTransition)
        {
            Source = source;
            Target = target;
            TransitValue = transitValue;
            ObjectTransition = objectTransition;
            ValueType = transitValue?.GetType();
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

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceEntry() {Mesage = msg, Color = color});
        }

        public  string GetInfo()
        {
            return "==============SRC==============\n" + Source.GetInfo()
                   + "\n==============TARGET==============\n" + Target?.GetInfo()
                   + "\n==============TransitValue==============\n" + ((TransitValue as IValuesObject)?.GetInfo() ?? TransitValue)
                   + "\n==============ValueType: " + ValueType;
        }
    }
}