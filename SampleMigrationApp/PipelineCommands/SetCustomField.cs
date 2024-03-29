using System;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;

namespace SampleMigrationApp.PipelineCommands
{
    public class SetCustomField : CommandBase
    {
        public string FieldId { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
           //do some logic
        }

        public static implicit operator SetCustomField(string fieldName)
        {
            return new SetCustomField() { FieldId = fieldName  };
        }
    }   
}