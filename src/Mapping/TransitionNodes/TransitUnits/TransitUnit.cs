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
            ConsoleColor = ConsoleColor.Gray;
        }

        protected override object TransitValueInternal(ValueTransitContext ctx)
        {
            var returnValue = ctx.TransitValue;
            if (Expression.IsNotEmpty())
            {
                returnValue = ExpressionEvaluator.Evaluate(Expression, ctx);
            }

            return returnValue;
        }

        public override string GetInfo()
        {
            return $"{base.GetInfo()}\n{GetIndent(5)}Expression: {Expression}";
        }
    }
}