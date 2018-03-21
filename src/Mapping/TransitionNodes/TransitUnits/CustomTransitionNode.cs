using System;
using System.Diagnostics;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class CustomTransitionNode : TransitionNode
    {
        private readonly Func<ValueTransitContext, TransitResult> _transitMethod;

        public CustomTransitionNode(Func<ValueTransitContext, TransitResult> transitMethod )
        {
            Debug.Assert(transitMethod != null);
            _transitMethod = transitMethod;
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            return _transitMethod(ctx);
        }
    }
}