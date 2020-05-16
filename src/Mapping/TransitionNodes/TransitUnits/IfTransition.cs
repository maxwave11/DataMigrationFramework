using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class IfTransition: TransitUnit<bool>
    {
        public TransitionNode OnTrue { get; set; } = new FlowTransition() { Flow = TransitionFlow.Continue };
        public TransitionNode OnFalse { get; set; } = new FlowTransition() { Flow = TransitionFlow.SkipValue };
        
        public override void Initialize(TransitionNode parent)
        {
            if (Expression == null)
                throw new Exception($"{nameof(Expression)} is required for { nameof(IfTransition)}");

            base.Initialize(parent);
        }
        
        protected override void TransitInternal(ValueTransitContext ctx)
        {
            if (Expression.Evaluate(ctx))
                OnTrue.Transit(ctx);
            else
                OnFalse.Transit(ctx);
        }

        public static implicit operator IfTransition(string expression)
        {
            return new IfTransition() { Expression = expression };
        }
    }
}
