using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    public class CommandAttribute: Attribute
    {
        public string Name { get; }
        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
    
    /// <summary>
    /// Base class for all transition elements in Map configuration
    /// </summary>
    public abstract class CommandBase
    {
        public ConsoleColor TraceColor { get; set; } = ConsoleColor.White;

        /// Method to override in client's code for custom commands. Allow to use custom logic.
        /// inherited from CommandBase class
        /// Don't call this method directly. Use Execute method of ValueTransitContext instead
        public abstract void ExecuteInternal(ValueTransitContext ctx);

        public virtual string GetParametersInfo() => string.Empty;
      

        public static implicit operator CommandBase(string expression)
        {
            return new GetValueCommand() { Expression = expression };
        }
    }

   
}