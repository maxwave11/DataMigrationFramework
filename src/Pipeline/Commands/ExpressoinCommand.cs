using XQ.DataMigration.Pipeline.Expressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    public class ExpressionCommand<T>: CommandBase
    {
        //workaround variable. Need to think how to refactor inheritance from CommandBase
        public  T ReturnValue { get; private set; }
        public MigrationExpression<T> Expression { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            ReturnValue = Expression.Evaluate(ctx);
            string valueType = ReturnValue?.GetType().Name.Truncate(30);
            ctx.TraceLine($"<- ({  valueType }){ReturnValue?.ToString().Truncate(30)}");
        }

        public static implicit operator ExpressionCommand<T>(string expression)
        {
            return new ExpressionCommand<T>() { Expression = expression };
        }

        public override string GetParametersInfo() => Expression.ToString();
    }
}