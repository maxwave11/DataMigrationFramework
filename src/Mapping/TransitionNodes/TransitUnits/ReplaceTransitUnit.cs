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
        public string ReplaceExpression { get; set; }

        // public override void Initialize(TransitionNode parent)
        // {
        //     if (ReplaceExpression.IsEmpty() && Pipeline?.Any() != true)
        //         throw new Exception($"Need to fill {nameof(ReplaceExpression)} or {nameof(Pipeline)}");
        //
        //     if (ReplaceExpression.IsNotEmpty())
        //     {
        //         if (Pipeline?.Any() == true)
        //             throw new Exception(nameof(ReplaceTransitUnit) + "not allows not empty Rules property while having child rules collection");
        //
        //         var rules = ReplaceExpression.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //
        //         Pipeline = rules.Select(i => new ReplaceStepUnit { Rule = i }).ToList();
        //     }
        //
        //     Pipeline.ForEach(r => r.Initialize(this));
        //
        //     base.Initialize(parent);
        // }

        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            TransitResult replaceResult = null;
            foreach (var childTransition in Pipeline)
            {
                replaceResult = childTransition.Transit(ctx);
                
                //if (replaceResult.Flow == TransitionFlow.SkipUnit)
                //{
                //    //if ReplaceUnit returned SkipUnit then need to stop replacing sequence
                //    break;
                //}

                if (replaceResult.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}", ctx);
                    return replaceResult;
                }
            }

            return replaceResult;
        }

        public override string ToString()
        {
            return base.ToString() + "ReplaceValue: " + ReplaceExpression;
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