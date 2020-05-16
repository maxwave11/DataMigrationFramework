using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TransitUnit : TransitUnit<object>
    {
    }
    /// <summary>
    /// TransitInternal unit is a node which can't contains nesting elements
    /// </summary>
    public class TransitUnit<T>: TransitionNode
    {
        public MigrationExpression<T> Expression { get; set; }

        protected override void TransitInternal(ValueTransitContext ctx)
        {
            var  returnValue = Expression.Evaluate(ctx);
            ctx.SetCurrentValue(returnValue);
        }
        
        public override string ToString()
        {
            return $"Expresion: { Expression }";
        }
    }
}