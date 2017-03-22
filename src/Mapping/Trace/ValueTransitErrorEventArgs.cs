using System;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Trace
{
    public class TransitErrorEventArgs
    {
        public TransitionNode ValueTransition { get; private set; }
        public ValueTransitContext Context { get; private set; }
        public bool Continue { get; set; }

        public TransitErrorEventArgs(TransitionNode valueTransition, ValueTransitContext context)
        {
            ValueTransition = valueTransition;
            Context = context;
        }

        public override string ToString()
        {
            try
            {
                string errorMsg = 
$@"Error description:
============ TRACE ========== 
{ Context.ObjectTransition.TraceEntries.Select(t=>t.Mesage).Join("\n") }
==============SRC==============
{ Context.Source?.GetInfo().Truncate(1024) }
==============TARGET===========
{ Context.Target?.GetInfo() }
==============TransitValue=====
{ ((Context.TransitValue as IValuesObject)?.GetInfo().Truncate(1024) ?? Context.TransitValue) }
==============ValueType: {Context.ValueType}";

                return errorMsg;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while getting context info: " + ex);
            }
        }
    }
}