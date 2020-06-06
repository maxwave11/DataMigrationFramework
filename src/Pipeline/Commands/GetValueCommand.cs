using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline.Expressions;

namespace XQ.DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Puts the value from expression to pipeline
    /// </summary>
    [Command("GET")]
    public class GetValueCommand : CommandBase
    {
        public MigrationExpression Expression { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            var returnValue = Expression.Evaluate(ctx);
            ctx.SetCurrentValue(returnValue);
        }

        public override string GetParametersInfo() => Expression.ToString();

        public static implicit operator GetValueCommand(string expression)
        {
            return new GetValueCommand() { Expression = expression };
        }
    }
    /// <summary>
    /// Puts the not empty value from expression to pipeline. Skip if value is empty
    /// </summary>
    [Command("GET?")]
    public sealed class GetNotEmptyValueCommand : GetValueCommand
    {
        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            base.ExecuteInternal(ctx);
            if (ExpressionContext.IsEmpty(ctx.TransitValue))
                ctx.Flow = TransitionFlow.SkipValue;
        }

        public static implicit operator GetNotEmptyValueCommand(string expression)
        {
            return new GetNotEmptyValueCommand() { Expression = expression };
        }
    } 
}