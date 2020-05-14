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
        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            if (Expression != null)
            {
                var returnValue = Expression.Evaluate(ctx);
                return new TransitResult(returnValue);
            }

            _key = "";
            base.TransitInternal(ctx);
            return new TransitResult(_key.TrimEnd('/'));
        }
        protected override TransitResult TransitChild(ReadTransitUnit childTransition, ValueTransitContext ctx)
        {
            var result = base.TransitChild(childTransition, ctx);
            _key += result.Value + "/";
            return new TransitResult(null);
        }

        public static implicit operator ReadTransitUnit(string expression)
        {
            var retVal = new ReadTransitUnit() { Expression = expression };
            return retVal;
        }

        public override string ToString()
        {
            if (Expression != null)
                return Expression.ToString();

            return $"[{ Pipeline.Select(i => i.ToString()).Join() }]";
        }
    }
}