using System;
using System.Diagnostics;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("FLOW")]
    public class SetFlowCommand: CommandBase
    {
        public TransitionFlow Flow { get; set; } = TransitionFlow.Continue;
        
        public string Message { get; set; }
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (Flow == TransitionFlow.Debug)
            {
                Debugger.Break();
                Flow = TransitionFlow.Continue;
            }

            if (Flow == TransitionFlow.RiseError)
                throw new Exception( Message.IsEmpty() ? "Error raised by Flow command!" : Message);

            ctx.Flow = Flow;
        }

        public override string GetParametersInfo() => Flow.ToString();

        public static implicit operator SetFlowCommand(string expression)
        {
            return new SetFlowCommand() { Flow = (TransitionFlow)Enum.Parse(typeof(TransitionFlow), expression) };
        }
    }
}