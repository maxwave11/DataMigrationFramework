using System;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TransitUnit: ValueTransitionBase
    {
        [XmlAttribute]
        public string Expression { get; set; }

        public TransitUnit()
        {
        }

        public override TransitResult TransitValue(ValueTransitContext ctx)
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
            return $"{base.ToString()}\n\tExpression: {Expression}";
        }
    }
}