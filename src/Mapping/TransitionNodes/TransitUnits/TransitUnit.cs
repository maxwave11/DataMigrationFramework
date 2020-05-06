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
    /// TransitInternal unit is a node which can't contains nesting elements
    /// </summary>
    public class TransitUnit: TransitionNode
    {
        public MigrationExpression Expression { get; set; }

        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            var  returnValue = Expression.Evaluate(ctx);
            return new TransitResult(returnValue);
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes += $" Expression: { Expression }";
            base.TraceStart(ctx, attributes);
        }
        
        public override string ToString()
        {
            return $"Type: {GetType().Name}, Expresion: { Expression }";
        }
       
    }
}