using System;
using System.Collections.Generic;
using DataMigration.Pipeline;
using DataMigration.Pipeline.Commands;

namespace SampleMigrationApp.PipelineCommands
{
    public class SetCashFlowValue : CommandBase
    {
        Dictionary<string, decimal?> _actualsAccumulator = new Dictionary<string, decimal?>();

        public ExpressionCommand<int> CashFlowItemId { get; set; }
        
        public ExpressionCommand<DateTime> Date { get; set; }

        public ExpressionCommand<bool> IsActuals { get; set; } = "true";
        public bool IsAnnualValue { get; set; } = false;
        public bool Accumulate { get; set; } = true;

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            //do something
        }
    }
}