using System;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.Mapping.Logic
{
    public class TransitResult: TransitResult<object>
    {
        public TransitResult(object value) : base(value)
        {
        }

        public TransitResult(TransitionFlow flow, object value, string message = "") : base(flow, value, message)
        {
        }
    }

    public class TransitResult<T>
    {
        public TransitResult(T value)
        {
            Value = value;
        }

        public TransitResult(TransitionFlow flow, T value, string message ="")
        {
            Flow = flow;
            Value = value;
            Message = message;
        }

        public TransitionFlow Flow { get; } = TransitionFlow.Continue;
        public T Value { get; }
        public string Message { get; }
    }
}