using System;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class Condition: ComplexTransition
    {
        [XmlAttribute]
        public string Expression { get; set; }

        [XmlAttribute]
        public TransitContinuation ActionOnTrue { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (Expression.IsEmpty())
                throw new Exception($"{nameof(Expression)} attribute is required for { nameof(Condition)} element");

            //Other actions except Continue not compatible with children execution. Ether we should excute action or run children nodes.
            //Continue action means that we use children transitions if Expression is true. In other cases we should not use child nodes
            //We can change this flow and run children before action but in may be extra complexity.
            //Also there is some idea to get rid of ActionOnTrue attribute and introduce new ChangeFlowUnit which will executed as 
            //child object
            if (ActionOnTrue != TransitContinuation.Continue && ChildTransitions.Count > 0)
                throw new Exception($"{nameof(Expression)} attribute is required for { nameof(Condition)} element");

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var boolValue = ExpressionEvaluator.Evaluate(Expression, ctx) as bool?;

            if (!boolValue.HasValue)
                throw new Exception($"Result of Expression execution in {nameof(Condition)} must be boolean");

            if (boolValue.Value == true)
                return base.Transit(ctx);
           
            return new TransitResult(ctx);
        }
    }
}
