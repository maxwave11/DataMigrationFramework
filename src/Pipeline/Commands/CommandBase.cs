using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string Name { get; set; }

        public ConsoleColor TraceColor { get; set; } = ConsoleColor.White;

        private bool _isValidated;
        private void Validate()
        {
            if (_isValidated)
                return;
            
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, new ValidationContext(this), results, true))
            {
                var firstError = results[0];
                throw new ValidationException(firstError, null, this);
            }

            _isValidated = true;
        }

        /// <summary>
        /// Main (core) transition method which wraps node transition logic by logging and next flow control
        /// Generally used by DataMigration classes to construct transition flow
        /// </summary>
        public void Execute(ValueTransitContext ctx)
        {
            Validate();
            
            ctx.CurrentNode = this;

            TraceStart(ctx);

            try
            {
                ExecuteInternal(ctx);
            }
            catch (Exception ex)
            {
                Migrator.Current.Tracer.TraceError(ex.ToString(), ctx);
                throw;
            }

            TraceEnd(ctx);
        }


        /// <summary>
        /// Method to override in client's code for custom transitions. Allow to use custom logic inside own transitino nodes
        /// inherited from CommandBase class
        /// Don't use this method inside XQ.DataMigration tool - use
        /// <code>Execute</code> instead
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected abstract void ExecuteInternal(ValueTransitContext ctx);

        protected  virtual void TraceStart(ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.Indent();

            string tagName = CommandUtils.GetCommandYamlName(GetType());

            var incomingValue = ctx.TransitValue?.ToString();
            string incomingValueType = ctx.TransitValue?.GetType().Name;

            TraceLine($"-> {tagName} { this.ToString() }", ctx);
            Migrator.Current.Tracer.Indent();
            TraceLine($"({incomingValueType.Truncate(30)}){incomingValue}", ctx);
        }

        protected  virtual void TraceEnd(ValueTransitContext ctx)
        {
            
            Migrator.Current.Tracer.IndentBack();

            var returnValue = ctx.TransitValue?.ToString();
            string returnValueType = ctx.TransitValue?.GetType().Name;

            string traceMsg = $"<- ({returnValueType.Truncate(30)}){returnValue}\n";
            TraceLine(traceMsg,ctx);

            Migrator.Current.Tracer.IndentBack();
        }

        protected  void TraceLine(string message, ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.TraceLine(message, ctx, this.TraceColor );
        }
        

        public static implicit operator CommandBase(string expression)
        {
            return new GetCommand() { Expression = expression };
        }
    }
}