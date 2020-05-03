using System;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class Condition: ComplexTransition
    {
        public MigrationExpression  Expression { get; set; }

        public TransitContinuation ActionOnTrue { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (Expression == null)
                throw new Exception($"{nameof(Expression)} is required for { nameof(Condition)}");

            //Other actions except Continue not compatible with children execution. Ether we should excute action or run children nodes.
            //Continue action means that we use children transitions if Expression is true. In other cases we should not use child nodes
            //We can change this flow and run children before action but in may be extra complexity.
            //Also there is some idea to get rid of ActionOnTrue attribute and introduce new ChangeFlowUnit which will executed as 
            //child object
            if (ActionOnTrue != TransitContinuation.Continue && Pipeline.Count > 0)
                throw new Exception($"{nameof(Expression)} is required for { nameof(Condition)} ");

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var boolValue = Expression.Evaluate(ctx) as bool?;

            if (!boolValue.HasValue)
                throw new Exception($"Result of Expression execution in {nameof(Condition)} node must be boolean");

            if (boolValue.Value == true)
            {
                if (ActionOnTrue != TransitContinuation.Continue)
                    return new TransitResult(ActionOnTrue, null);

                return base.Transit(ctx);
            }

            return new TransitResult(ctx.TransitValue);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes = $"Expression=\"{Expression}\"";
            base.TraceStart(ctx, attributes);
        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            //Reset TransitValue by Source object before any children begins inside ObjectTrastition
            //Notice: if you want to pass TransitValue between transitions you have to place your
            //'connected' transition nodes inside ValueTransition
            ctx.SetCurrentValue(childNode.Name, ctx.Source);

            return base.TransitChild(childNode, ctx);
        }
    }
}
