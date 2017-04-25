using System;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TransitUnit: TransitionNode
    {
        [XmlAttribute]
        public string Expression { get; set; }

        internal Expressions.ExpressionEvaluator ExpressionEvaluator { get; } = new Expressions.ExpressionEvaluator();

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var returnValue = ctx.TransitValue;
            if (Expression.IsNotEmpty())
            {
                returnValue = ExpressionEvaluator.Evaluate(Expression, ctx);
            }

            return new TransitResult(TransitContinuation.Continue, returnValue);
        }

        public override string ToString()
        {
            return $"{base.ToString()}\n    Expression: {Expression}";
        }
    }
}