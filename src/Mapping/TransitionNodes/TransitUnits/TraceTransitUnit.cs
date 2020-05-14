using System;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TraceTransitUnit : TransitionNode
    {
        public bool Trace { get; set; }
        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.TraceEnabled = this.Trace;
            return new TransitResult(ctx.TransitValue);
        }
        
        public static implicit operator TraceTransitUnit(string expression)
        {
            return new TraceTransitUnit() { Trace = !expression.IsNotEmpty() || bool.Parse(expression)  };
        }
    }
}