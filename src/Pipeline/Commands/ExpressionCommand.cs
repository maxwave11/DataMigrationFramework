using XQ.DataMigration.Pipeline.Expressions;
using YamlDotNet.Serialization;

namespace XQ.DataMigration.Pipeline.Commands
{
    public class ExpressionCommand : ExpressionCommand<object>
    {
    }
    /// <summary>
    /// ExecuteInternal unit is a node which can't contains nesting elements
    /// </summary>
    public class ExpressionCommand<T>: CommandBase
    {
        public MigrationExpression<T> Expression { get; set; }

        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            var  returnValue = Expression.Evaluate(ctx);
            ctx.SetCurrentValue(returnValue);
        }
        
        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}