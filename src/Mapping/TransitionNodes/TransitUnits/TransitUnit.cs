using System;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{

    /// <summary>
    /// Transit unit is a node which can't contains nesting elements
    /// </summary>
    public class TransitUnit: TransitionNode
    {
        public MigrationExpression Expression { get; set; }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var  returnValue = Expression.Evaluate(ctx);
            return new TransitResult(returnValue);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes += $" Expression: { Expression }";
            base.TraceStart(ctx, attributes);
        }
        
        public static implicit operator TransitUnit(string expression)
        {
            return new TransitUnit() { Expression = expression };
        }
    }
}