using System;
using System.ComponentModel.DataAnnotations;
using DataMigration.Enums;
using DataMigration.Pipeline.Expressions;

namespace DataMigration.Pipeline.Commands
{
    [Command("IF")]
    public class IfCommand: CommandBase
    {
        [Required]
        public MigrationExpression<bool> Condition { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (!Condition.Evaluate(ctx))
                ctx.Flow = TransitionFlow.SkipValue;
        }

        public override string GetParametersInfo()
        {
            return Condition.ToString();
        }

        public static implicit operator IfCommand(string expression)
        {
            //default IF initialization from plain string
            return new IfCommand() {  Condition = expression };
        }
    }
}
