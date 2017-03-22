using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition which allows to add custom messages to migration trace
    /// </summary>
    public class WriteMessageUnit: TransitUnit
    {
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var result =  base.Transit(ctx);
            if (result.Continuation == TransitContinuation.Continue)
                Migrator.Current.Tracer.TraceText(result.Value?.ToString(), this);
            return result;
        }
    }
}
