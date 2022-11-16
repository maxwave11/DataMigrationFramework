using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Assign TAG name to the command. You can use it in YAML configuration
    /// instead of full command class name
    /// </summary>
    public class CommandAttribute: Attribute
    {
        public string Name { get; }
        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
    
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