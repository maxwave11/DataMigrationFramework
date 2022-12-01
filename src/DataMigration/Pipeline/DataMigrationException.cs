﻿using System;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.Interfaces;
using DataMigration.Utils;

namespace DataMigration.Pipeline
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
{ Context.Source }

==============TARGET===========
{ Context.Target }

==============TransitValue=====
{  Context.TransitValue }

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