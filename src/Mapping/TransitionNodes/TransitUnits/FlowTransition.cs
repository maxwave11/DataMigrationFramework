using System;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class FlowTransition: TransitionNode
    {
        public TransitionFlow Flow { get; set; } = TransitionFlow.Continue;
        public override TransitResult Transit(ValueTransitContext ctx)
        {
            return new TransitResult(Flow, null);
        }
        
        public static implicit operator FlowTransition(string expression)
        {
            return new FlowTransition() { Flow = (TransitionFlow)Enum.Parse(typeof(TransitionFlow), expression) };
        }
    }
}