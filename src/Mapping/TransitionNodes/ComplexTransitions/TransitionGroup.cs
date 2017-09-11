using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions
{
    public class TransitionGroup : ComplexTransition
    {
        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            return base.TransitChild(childNode, new ValueTransitContext(null,null,null,null));
        }
    }
}