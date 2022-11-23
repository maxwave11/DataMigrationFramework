using System;
using System.Linq.Expressions;
using DataMigration.Pipeline.Expressions;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// ExecuteInternal unit which writes incoming value from ValueTransitContext to target object
    /// If Expression is just a property name -> unit writes value to appropriate property of target object
    /// If Expression is Migration expression -> unit executes this expression
    /// </summary>
    [Yaml("SET")]
    [Obsolete]
    public class SetCommand : CommandBase
    {
        public string ToField { get; set; }
        
        //public MigrationExpression Expression { get; set; }
        
        private readonly Action<ValueTransitContext> _func;
        private Expression<Action<ValueTransitContext>> _expression2 { get; }

        public SetCommand(Expression<Action<ValueTransitContext>> expression2)
        {
            _expression2 = expression2;
            _func = _expression2.Compile();
        }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            _func(ctx);
        }

        // public override void ExecuteInternal(ValueTransitContext ctx)
        // {
        //     if (ToField.IsNotEmpty())
        //         ctx.Target.SetValue(ToField, ctx.TransitValue);
        //     else
        //         Expression.Evaluate(ctx);
        // }
        
        // public static implicit operator SetCommand(string expression)
        // {
        //     if (expression.IsEmpty())
        //         throw new InvalidOperationException("Expression can't be empty");
        //     
        //     if (expression.StartsWith("=>"))
        //         return new SetCommand() { Expression = expression.TrimStart('=','>') };
        //     
        //     return new SetCommand() { ToField = expression };
        // }
        
        public override string GetParametersInfo() => ToField.IsNotEmpty() ? $"ToField: {ToField}" : _expression2.GetExpressionText();
    }   
    
    
}