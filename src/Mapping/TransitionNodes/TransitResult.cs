namespace XQ.DataMigration.Mapping.TransitionNodes
{
    public enum TransitContinuation { Continue, SkipUnit, SkipValue, SkipObject, Stop, RaiseError }

    public class TransitResult
    {
        public TransitResult()
        {
            Continuation = TransitContinuation.Continue;
        }

        public TransitResult(TransitContinuation continuation, object value, string message ="")
        {
            Continuation = continuation;
            Value = value;
            Message = message;
        }

        public TransitContinuation Continuation { get; }
        public object Value { get; }
        public string Message { get; }
    }
}