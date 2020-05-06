using System;
using System.Diagnostics;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class FlowTransition: TransitionNode
    {
        public TransitionFlow Flow { get; set; } = TransitionFlow.Continue;
        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            if (Flow == TransitionFlow.Debug)
            {
                Debugger.Break();
                Flow = TransitionFlow.Continue;
            }
            
            return new TransitResult(Flow, ctx.TransitValue);
        }
        
        public static implicit operator FlowTransition(string expression)
        {
            return new FlowTransition() { Flow = (TransitionFlow)Enum.Parse(typeof(TransitionFlow), expression) };
        }
    }
}