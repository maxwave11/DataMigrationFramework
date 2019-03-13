using System;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ConditionUnit: TransitUnit
    {
        [XmlAttribute]
        public string Message { get; set; }

        [XmlAttribute]
        public TransitContinuation OnTrue { get; set; }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var transitValue = ctx.TransitValue;
            var transitResult = base.Transit(ctx);

            var boolValue = transitResult.Value as bool?;
            if (!boolValue.HasValue)
                throw new Exception($"Result of Expression execution in {nameof(ConditionUnit)} must have Boolean type");

            TransitContinuation continuation;
            string ruleMessage = "";
            TraceLine($"Is valid: {boolValue.Value}");
            if (boolValue.Value == true)
            {
                continuation = OnTrue;
                var defaultMessage = $"The value={ctx.TransitValue} didn't pass condition expression {Expression}";
                ruleMessage = Message ?? defaultMessage;
            }
            else
            {
                continuation = TransitContinuation.Continue;
            }
            return new TransitResult(continuation, transitValue, ruleMessage);
        }
    }
}
