using XQ.DataMigration.Pipeline.Expressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("GET")]
    public sealed class GetCommand : CommandBase
    {
        public MigrationExpression Expression { get; set; }

        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            var returnValue = Expression.Evaluate(ctx);
            ctx.SetCurrentValue(returnValue);
        }
        
        public static implicit operator GetCommand(string expression)
        {
            return new GetCommand() { Expression = expression };
        }
    }

    public class ExpressionCommand<T>: CommandBase
    {
        //workaround variable. Need to think how to refactor inheritance from CommandBase
        private T _returnValue;
        public MigrationExpression<T> Expression { get; set; }

        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            _returnValue = Expression.Evaluate(ctx);
        }

        protected override void TraceEnd(ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.IndentBack();

            var valueType = _returnValue.GetType().Name.Truncate(30);
            TraceLine($"<- ({  valueType }){_returnValue?.ToString().Truncate(30)}" , ctx);
            
            Migrator.Current.Tracer.IndentBack();

        }

        public new T Execute(ValueTransitContext ctx)
        {
            base.Execute(ctx);
            return _returnValue;
        }
        
        public static implicit operator ExpressionCommand<T>(string expression)
        {
            return new ExpressionCommand<T>() { Expression = expression };
        }

        public override string ToString() => Expression.ToString();
    }
}