using System.Linq;
using XQ.DataMigration.Pipeline.Expressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{    
    [Command("GET")]
    public class GetCommandSet : CommandSet<GetCommandSet>
    {
        public MigrationExpression Expression { get; set; }
        string _key = "";
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (Expression != null)
            {
                var returnValue = Expression.Evaluate(ctx);
                ctx.SetCurrentValue(returnValue);
                return;
            }

            _key = "";
            base.ExecuteInternal(ctx);
            ctx.SetCurrentValue(_key.TrimEnd('/'));
        }
        protected override void TransitChild(GetCommandSet childTransition, ValueTransitContext ctx)
        {
            base.TransitChild(childTransition, ctx);
            _key += ctx.TransitValue + "/";
        }

        public static implicit operator GetCommandSet(string expression)
        {
            var retVal = new GetCommandSet() { Expression = expression };
            return retVal;
        }

        public override string ToString()
        {
            if (Expression != null)
                return "Expression: " + Expression.ToString();

            return $"[{ Pipeline.Select(i => i.ToString()).Join() }]";
        }
    }
}