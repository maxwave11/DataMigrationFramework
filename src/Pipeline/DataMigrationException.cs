﻿using System;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline
{
    public class DataMigrationException: Exception 
    {
        public ValueTransitContext Context { get; }
        public DataMigrationException(string message, ValueTransitContext ctx, Exception innerException) : base(message, innerException)
        {
            Context = ctx;
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
{ Context.TraceEntries.Select(t => t.Text).Join("") }

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
                return "Error while parsing context info:" + ex;
            }
        }
    }
}