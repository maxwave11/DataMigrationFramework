using System;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TraceTransitUnit : TransitionNode
    {
        public bool Trace { get; set; }
        protected override void TransitInternal(ValueTransitContext ctx)
        {
            ctx.Trace = this.Trace;
        }
        
        public static implicit operator TraceTransitUnit(string expression)
        {
            return new TraceTransitUnit() { Trace = !expression.IsNotEmpty() || bool.Parse(expression)  };
        }
    }
}