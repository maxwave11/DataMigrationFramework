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
        public ComplexTransition<TransitionNode> OnTrueComplex { get; set; }
        //public TransitionNode OnFalse { get; set; } = new FlowTransition() { Flow = TransitionFlow.Continue };
        
        public override void Initialize(TransitionNode parent)
        {
            if (Expression == null)
                throw new Exception($"{nameof(Expression)} is required for { nameof(IfTransition)}");
            if (OnTrueComplex != null)
                OnTrue = OnTrueComplex;

            base.Initialize(parent);
        }

        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            bool boolValue = Expression.Evaluate(ctx);

            return boolValue
                ? OnTrue.Transit(ctx)
                : new TransitResult(TransitionFlow.Continue, ctx.TransitValue);
        }

        public static implicit operator IfTransition(string expression)
        {
            return new IfTransition() { Expression = expression };
        }

        //protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        //{
        //    attributes = $"Expression=\"{Expression}\"";
        //    base.TraceStart(ctx, attributes);
        //}

        //protected override TransitResult TransitValue(TransitionNode childNode, ValueTransitContext ctx)
        //{
        //    //Reset TransitValue by Source object before any children begins inside ObjectTrastition
        //    //Notice: if you want to pass TransitValue between transitions you have to place your
        //    //'connected' transition nodes inside ValueTransition
        //    ctx.SetCurrentValue(childNode.Name, ctx.Source);

        //    return base.TransitValue(childNode, ctx);
        //}
    }
}
