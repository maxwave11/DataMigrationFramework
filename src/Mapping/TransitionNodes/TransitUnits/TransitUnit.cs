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

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var returnValue = ctx.TransitValue;
            if (Expression.IsNotEmpty())
            {
                returnValue = ExpressionEvaluator.Evaluate(Expression, ctx);
            }

            return new TransitResult(returnValue);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes += $" Expression=\"{ Expression }\"";
            base.TraceStart(ctx, attributes);
        }
    }
}