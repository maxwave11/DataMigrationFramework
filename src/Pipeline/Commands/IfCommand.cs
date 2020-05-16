using System;
using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("IF")]
    public class IfCommand: ExpressionCommand<bool>
    {
        public CommandBase OnTrue { get; set; } = new SetFlowCommand() { Flow = TransitionFlow.Continue };
        public CommandBase OnFalse { get; set; } = new SetFlowCommand() { Flow = TransitionFlow.SkipValue };
        
        public override void Initialize(CommandBase parent)
        {
            if (Expression == null)
                throw new Exception($"{nameof(Expression)} is required for { nameof(IfCommand)}");

            base.Initialize(parent);
        }
        
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (Expression.Evaluate(ctx))
                OnTrue.Execute(ctx);
            else
                OnFalse.Execute(ctx);
        }

        public static implicit operator IfCommand(string expression)
        {
            return new IfCommand() { Expression = expression };
        }
    }
}
