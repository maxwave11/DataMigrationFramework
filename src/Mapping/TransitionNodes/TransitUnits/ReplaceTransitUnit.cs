using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions
{
    public class ReplaceTransitUnit : ComplexTransition<ReplaceStepUnit>
    {
        protected override void TransitInternal(ValueTransitContext ctx)
        {
            foreach (var childTransition in Pipeline)
            {
                childTransition.Transit(ctx);

                if (ctx.Flow == TransitionFlow.SkipValue)
                {
                    //if ReplaceUnit returned SkipValue then need to stop ONLY replacing sequence (hack, need to refactor to do
                    //it in more convenient way
                    ctx.Flow = TransitionFlow.Continue;
                    break;
                }

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}", ctx);
                    break;
                }
            }
        }


        public static implicit operator ReplaceTransitUnit(string expression)
        {
            if (expression.IsEmpty())
                throw new Exception("Replace expression cant be empty");

            var rules = expression.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return new ReplaceTransitUnit() { Pipeline = rules.Select(i=> (ReplaceStepUnit)i).ToList()};
        }
    }
}