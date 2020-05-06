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
    public class IfTransition: TransitUnit
    {
        public TransitionNode OnTrue { get; set; } = new FlowTransition() { Flow = TransitionFlow.Continue };
        public TransitionNode OnFalse { get; set; } = new FlowTransition() { Flow = TransitionFlow.SkipValue };
        
        public override void Initialize(TransitionNode parent)
        {
            if (Expression == null)
                throw new Exception($"{nameof(Expression)} is required for { nameof(IfTransition)}");

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var boolValue = Expression.Evaluate(ctx) as bool?;

            if (!boolValue.HasValue)
                throw new Exception($"Result of Expression execution in {nameof(IfTransition)} node must be boolean");
            

            return boolValue.Value ? OnTrue.Transit(ctx) : OnFalse.Transit(ctx);
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

        //protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        //{
        //    //Reset TransitValue by Source object before any children begins inside ObjectTrastition
        //    //Notice: if you want to pass TransitValue between transitions you have to place your
        //    //'connected' transition nodes inside ValueTransition
        //    ctx.SetCurrentValue(childNode.Name, ctx.Source);

        //    return base.TransitChild(childNode, ctx);
        //}
    }
}
