namespace XQ.DataMigration.Mapping.TransitionNodes
{
    public enum TransitContinuation { Continue, SkipUnit, SkipValue, SkipObject, Stop, RaiseError }

    public class TransitResult
    {
        public TransitResult(TransitContinuation continuation, object value)
        {
            Continuation = continuation;
            Value = value;
        }

        public TransitContinuation Continuation { get; }
        public object Value { get; }
    }
}