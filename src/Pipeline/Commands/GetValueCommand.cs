using DataMigration.Enums;
using DataMigration.Pipeline.Expressions;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Puts the value from expression to pipeline
    /// </summary>
    [Yaml("GET")]
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
    /// Puts not empty value from expression to pipeline. Do nothing if value empty
    /// </summary>
    [Yaml("GET?")]
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