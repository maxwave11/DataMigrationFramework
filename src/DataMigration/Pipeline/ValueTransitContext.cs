using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataMigration.Data;
using DataMigration.Enums;
using DataMigration.Pipeline.Commands;
using DataMigration.Pipeline.Trace;
using DataMigration.Utils;

namespace DataMigration.Pipeline
{
    public class ValueTransitContext
    {
        public IDataObject Source { get; }
        public IDataObject Target { get; set; }

        public TransitionFlow Flow { get; set; }

        public object TransitValue { get; private set; }
        
       // public DataPipeline DataPipeline { get; set; }
        
        public CommandBase CurrentCommand { get; set; }

        public readonly List<TraceMessage> TraceEntries = new List<TraceMessage>();
        
        // For debug purposes
        public string _traceEntries => TraceEntries.Select(i => i.Text).Join(" ");

        public bool Trace { get; set; }

        internal void AddTraceEntry(string msg, ConsoleColor color)
        {
            TraceEntries.Add(new TraceMessage(msg, color));
        }

        public ValueTransitContext(IDataObject source, object transitValue)
        {
            Source = source;
            TransitValue = transitValue;
        }

        public void SetCurrentValue(object value)
        {
            TransitValue = value;
            var valueType = TransitValue?.GetType().Name.Truncate(30);
            string message = $"==> ({valueType}){TransitValue?.ToString().Truncate(240) ?? "null" }";
            Migrator.Current.Tracer.TraceLine(message, this, ConsoleColor.DarkGray);
        }
        
        public void ResetCurrentValue()
        {
            TransitValue = null;
        }

        public void Execute(CommandBase cmd)
        {
            Validate(cmd);
            CurrentCommand = cmd;

            TraceLine($"{ CommandUtils.GetCommandYamlName(cmd.GetType()) } { cmd.GetParametersInfo() }");
            Migrator.Current.Tracer.Indent();
            
            cmd.ExecuteInternal(this);
          
            Migrator.Current.Tracer.IndentBack();
        }

        public T Execute<T>(ExpressionCommand<T> cmd)
        {
            Execute((CommandBase)cmd);
            return cmd.ReturnValue;
        }

        public void TraceLine(string message)
        {
            Migrator.Current.Tracer.TraceLine(message, this, CurrentCommand?.TraceColor ?? ConsoleColor.White);
        }
        
        
        private List<CommandBase> _validatedCommands = new List<CommandBase>(); 
        private void Validate(CommandBase cmd)
        {
            if (_validatedCommands.Contains(cmd))
                return;
            
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(cmd, new ValidationContext(cmd), results, true))
            {
                var firstError = results[0];
                throw new ValidationException(firstError, null, cmd);
            }

            _validatedCommands.Add(cmd);
        }
    }
}