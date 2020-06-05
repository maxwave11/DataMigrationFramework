using System;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Trace
{
    public class TransitErrorEventArgs
    {
        public ValueTransitContext Context { get; private set; }
        public bool Continue { get; set; }

        public TransitErrorEventArgs(ValueTransitContext context)
        {
            Context = context;
        }

        public override string ToString()
        {
            if (Context == null)
                return $"NULL {nameof(ValueTransitContext)}";
            try
            {
                string errorMsg = 
$@"Error description:
============ TRACE ========== 
{ Context.TraceEntries.Select(t=>t.Text).Join("") }

==============SRC==============
{ Context.Source?.GetInfo() }

==============TARGET===========
{ Context.Target?.GetInfo() }

==============TransitValue=====
{ ((Context.TransitValue as IDataObject)?.GetInfo().Truncate(1024) ?? Context.TransitValue) }

==============ValueType: {Context.TransitValue?.GetType()}";

                return errorMsg;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while getting context info: " + ex);
            }
        }
    }
}