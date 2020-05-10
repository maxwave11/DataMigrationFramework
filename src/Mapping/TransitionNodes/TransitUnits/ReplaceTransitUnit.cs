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
        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            TransitResult replaceResult = null;
            foreach (var childTransition in Pipeline)
            {
                replaceResult = childTransition.Transit(ctx);

                if (replaceResult.Flow == TransitionFlow.SkipValue)
                {
                    //if ReplaceUnit returned SkipUnit then need to stop replacing sequence

                    return new TransitResult(TransitionFlow.Continue, replaceResult.Value);
                }

                if (replaceResult.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}", ctx);
                    return replaceResult;
                }
            }

            return replaceResult;
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