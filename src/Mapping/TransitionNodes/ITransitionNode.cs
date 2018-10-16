using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    public interface ITransitionNode
    {
        TransitResult Transit(ValueTransitContext ctx);
    }
}