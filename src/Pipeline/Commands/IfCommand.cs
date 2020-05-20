using System;
using System.ComponentModel.DataAnnotations;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline.Expressions;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("IF")]
    public class IfCommand: CommandBase
    {
        [Required]
        public CommandBase OnTrue { get; set; }
        
        [Required]
        public CommandBase OnFalse { get; set; }
        
        [Required]
        public MigrationExpression<bool> Condition { get; set; }
        
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (Condition.Evaluate(ctx))
                OnTrue.Execute(ctx);
            else
                OnFalse.Execute(ctx);
        }

        public override string GetParametersInfo()
        {
            return Condition.ToString();
        }

        public static implicit operator IfCommand(string expression)
        {
            //default IF initialization from plain string
            return new IfCommand()
            {
                Condition = expression,
                OnTrue = new SetFlowCommand() { Flow = TransitionFlow.Continue },
                OnFalse = new SetFlowCommand() { Flow = TransitionFlow.SkipValue }
            };
        }
    }
}
