using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    public class TransitionGroup : ComplexTransition
    {
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            foreach (var objTransition in ChildTransitions)
            {
                if (!objTransition.Enabled)
                    continue;

                if (objTransition is GlobalObjectTransition)
                {
                    objTransition.Transit(null);
                }
                else
                {
                    objTransition.Transit(null);
                }
            }

            return new TransitResult();
        }
    }
}