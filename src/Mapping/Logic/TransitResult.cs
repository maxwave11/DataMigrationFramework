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

        public TransitResult(TransitContinuation continuation, object value, string message ="")
        {
            Continuation = continuation;
            Value = value;
            Message = message;
        }

        public TransitContinuation Continuation { get; } = TransitContinuation.Continue;
        public object Value { get; }
        public string Message { get; }
    }
}