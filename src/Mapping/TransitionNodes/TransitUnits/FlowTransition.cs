using System;
using System.Diagnostics;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class FlowTransition: TransitionNode
    {
        public TransitionFlow Flow { get; set; } = TransitionFlow.Continue;
        protected override void TransitInternal(ValueTransitContext ctx)
        {
            if (Flow == TransitionFlow.Debug)
            {
                Debugger.Break();
                Flow = TransitionFlow.Continue;
            }

            if (ctx.Flow != Flow)
            {
                TraceLine("Changing flow - " + Flow, ctx);
                ctx.Flow = Flow;

            }
        }

        public override string ToString()
        {
            return $"Flow: { Flow }";
        }

        public static implicit operator FlowTransition(string expression)
        {
            return new FlowTransition() { Flow = (TransitionFlow)Enum.Parse(typeof(TransitionFlow), expression) };
        }
    }
}