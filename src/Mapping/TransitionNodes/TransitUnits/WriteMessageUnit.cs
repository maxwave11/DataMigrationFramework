using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition which allows to write custom messages to migration trace
    /// </summary>
    public class WriteMessageUnit: TransitUnit
    {
        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            var result =  base.TransitInternal(ctx);
            if (result.Flow == TransitionFlow.Continue)
                TraceLine(result.Value?.ToString(), ctx);
            return result;
        }
    }
}
