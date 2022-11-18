using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Base class for all commands in migration configuration
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