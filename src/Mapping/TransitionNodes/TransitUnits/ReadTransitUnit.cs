using System.Linq;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReadTransitUnit : ComplexTransition<ReadTransitUnit>
    {
        public MigrationExpression Expression { get; set; }
        string _key = "";
        protected override void TransitInternal(ValueTransitContext ctx)
        {
            if (Expression != null)
            {
                var returnValue = Expression.Evaluate(ctx);
                ctx.SetCurrentValue(returnValue);
                return;
            }

            _key = "";
            base.TransitInternal(ctx);
            ctx.SetCurrentValue(_key.TrimEnd('/'));
        }
        protected override void TransitChild(ReadTransitUnit childTransition, ValueTransitContext ctx)
        {
            base.TransitChild(childTransition, ctx);
            _key += ctx.TransitValue + "/";
        }

        public static implicit operator ReadTransitUnit(string expression)
        {
            var retVal = new ReadTransitUnit() { Expression = expression };
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