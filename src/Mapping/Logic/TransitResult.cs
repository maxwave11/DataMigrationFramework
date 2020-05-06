using System;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.Mapping.Logic
{
    public class TransitResult
    {
        public TransitResult(object value)
        {
            Value = value;
        }

        public TransitResult(TransitionFlow flow, object value, string message ="")
        {
            Flow = flow;
            Value = value;
            Message = message;
        }

        public TransitionFlow Flow { get; } = TransitionFlow.Continue;
        public object Value { get; }
        public string Message { get; }
    }
}