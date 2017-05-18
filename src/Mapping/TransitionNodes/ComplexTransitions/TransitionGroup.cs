using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions
{
    public class TransitionGroup : ComplexTransition
    {
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            foreach (var objTransition in ChildTransitions)
            {
                if (!objTransition.Enabled)
                    continue;

                var result = objTransition.Transit(null);
                if (result.Continuation != TransitContinuation.Continue)
                    break;
            }

            return new TransitResult();
        }
    }
}