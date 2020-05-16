using System;
using System.Diagnostics;
using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Pipeline.Commands
{
    
    [Command("FLOW")]
    public class SetFlowCommand: CommandBase
    {
        public TransitionFlow Flow { get; set; } = TransitionFlow.Continue;
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (Flow == TransitionFlow.Debug)
            {
                Debugger.Break();
                Flow = TransitionFlow.Continue;
            }

            if (ctx.Flow == Flow) return;
            
            TraceLine("Changing flow - " + Flow, ctx);
            ctx.Flow = Flow;
        }

        public override string ToString()
        {
            return Flow.ToString();
        }

        public static implicit operator SetFlowCommand(string expression)
        {
            return new SetFlowCommand() { Flow = (TransitionFlow)Enum.Parse(typeof(TransitionFlow), expression) };
        }
    }
}