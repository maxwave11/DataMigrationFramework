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
    public class ReplaceTransitUnit : TransitUnit
    {
        [XmlAttribute]
        public string ReplaceRules { get; set; }

        [XmlArray("ReplaceUnits")]
        [XmlArrayItem(nameof(ReplaceStepUnit))]
        public List<ReplaceStepUnit> ReplaceUnits { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (ReplaceRules.IsEmpty() && ReplaceUnits?.Any() != true)
                throw new Exception($"Need to fill {nameof(ReplaceRules)} or {nameof(ReplaceUnits)}");

            if (ReplaceRules.IsNotEmpty())
            {
                if (ReplaceUnits?.Any() == true)
                    throw new Exception(nameof(ReplaceTransitUnit) + "not allows not empty Rules property while having child rules collection");

                var rules = ReplaceRules.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                ReplaceUnits = rules.Select(i => new ReplaceStepUnit { Rule = i }).ToList();
            }

            ReplaceUnits.ForEach(r => r.Initialize(this));

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            TransitResult replaceResult = null;
            foreach (var childTransition in ReplaceUnits)
            {
                if (!childTransition.CanTransit(ctx))
                    continue;

                replaceResult = childTransition.Transit(ctx);
                
                if (replaceResult.Continuation == TransitContinuation.SkipUnit)
                {
                    //if ReplaceUnit returned SkipUnit then need to stop replacing sequence
                    break;
                }

                if (replaceResult.Continuation != TransitContinuation.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}");
                    return replaceResult;
                }
            }

            return replaceResult;
        }

        public override string ToString()
        {
            return base.ToString() + "ReplaceRules: " + ReplaceRules;
        }
    }
}