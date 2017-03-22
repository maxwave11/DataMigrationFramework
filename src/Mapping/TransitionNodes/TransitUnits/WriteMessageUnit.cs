using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition which allows to add custom messages to migration trace
    /// </summary>
    public class WriteMessageUnit: TransitUnit
    {
        protected override void TraceStart(ValueTransitContext ctx)
        {
            //don't do anything because this Unit should only show message from Expression
            //and don't trace start and end of this transtit unit    
        }

        protected override void TraceEnd(ValueTransitContext ctx)
        {
            //don't do anything because this Unit should only show message from Expression
            //and don't trace start and end of this transtit unit    
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var result =  base.Transit(ctx);
            if (result.Continuation == TransitContinuation.Continue)
                Migrator.Current.Tracer.TraceText(result.Value?.ToString(), this);
            return result;
        }
    }
}
