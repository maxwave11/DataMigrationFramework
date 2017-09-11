﻿using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition which allows to write custom messages to migration trace
    /// </summary>
    public class WriteMessageUnit: TransitUnit
    {
        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            //don't do anything because this Unit should only show message from Expression
            //don't trace start and end of this TranstitUnit    
        }

        protected override void TraceEnd(ValueTransitContext ctx)
        {
            //don't do anything because this Unit should only show message from Expression
            //don't trace start and end of this TranstitUnit    
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var result =  base.Transit(ctx);
            if (result.Continuation == TransitContinuation.Continue)
                TraceLine(result.Value?.ToString());
            return result;
        }
    }
}
