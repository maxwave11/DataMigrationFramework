using System;
using System.ComponentModel.DataAnnotations;
using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("IF")]
    public class IfCommand: ExpressionCommand<bool>
    {
        [Required]
        public CommandBase OnTrue { get; set; }
        
        [Required]
        public CommandBase OnFalse { get; set; }
        
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (Expression.Evaluate(ctx))
                OnTrue.Execute(ctx);
            else
                OnFalse.Execute(ctx);
        }

        public static implicit operator IfCommand(string expression)
        {
            //default IF initialization from plain string
            return new IfCommand()
            {
                Expression = expression,
                OnTrue = new SetFlowCommand() { Flow = TransitionFlow.Continue },
                OnFalse = new SetFlowCommand() { Flow = TransitionFlow.SkipValue }
            };
        }
    }
}
