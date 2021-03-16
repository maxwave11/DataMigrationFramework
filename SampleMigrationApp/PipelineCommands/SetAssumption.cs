using System;
using XQ.DataMigration.Pipeline;
using XQ.DataMigration.Pipeline.Commands;

namespace SampleMigrationApp.PipelineCommands
{
    public class SetAssumption : CommandBase
    {
        public string Assumption { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
           //do somehing
        }

        public static implicit operator SetAssumption(string assumption)
        {
            return new SetAssumption() { Assumption = assumption };
        }

        public override string GetParametersInfo() => Assumption;
    }
}
