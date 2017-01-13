using System;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class RuleUnit: TransitUnit
    {
        [XmlAttribute]
        public string Message { get; set; }

        [XmlAttribute]
        public TransitContinuation Continuation { get; set; }

        public override TransitResult TransitValue(ValueTransitContext ctx)
        {
            var transitValue = ctx.TransitValue;
            var transitResult = base.TransitValue(ctx);

            var boolValue = transitResult.Value as bool?;
            if (!boolValue.HasValue)
                throw new Exception("Result of Expression execution in RuleUnit must be bool");

            TransitContinuation continuation;
            string ruleMessage = "";
            if (boolValue.Value)
            {
                continuation = Continuation;
                ruleMessage = $"The condition {Expression} is true for TransitValue={ctx.TransitValue}." + Message;
            }
            else
            {
                continuation = TransitContinuation.Continue;
            }
            return new TransitResult(continuation, transitValue);
        }
    }
}
