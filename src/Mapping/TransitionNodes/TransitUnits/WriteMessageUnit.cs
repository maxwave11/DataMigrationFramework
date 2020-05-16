using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition which allows to write custom messages to migration trace
    /// </summary>
    public class WriteMessageUnit: TransitUnit
    {
        protected  override void TransitInternal(ValueTransitContext ctx)
        {
            base.TransitInternal(ctx);
            if (ctx.Flow == TransitionFlow.Continue)
                TraceLine(ctx.TransitValue?.ToString(), ctx);
        }
    }
}
