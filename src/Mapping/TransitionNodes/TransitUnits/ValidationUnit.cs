using System;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ValidationUnit: TransitUnit
    {
        [XmlAttribute]
        public string Message { get; set; }

        [XmlAttribute]
        public TransitContinuation OnInvalid { get; set; }

        public override TransitResult TransitValue(ValueTransitContext ctx)
        {
            var transitValue = ctx.TransitValue;
            var transitResult = base.TransitValue(ctx);

            var boolValue = transitResult.Value as bool?;
            if (!boolValue.HasValue)
                throw new Exception($"Result of Expression execution in {nameof(ValidationUnit)} must have Boolean type");

            TransitContinuation continuation;
            string ruleMessage = "";
            if (boolValue.Value == false)
            {
                continuation = OnInvalid;
                var defaultMessage = $"The value={ctx.TransitValue} didn't pass validation expression {Expression}";
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
