using System;
using DataMigration.Data.DataSources;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Commands
{
    [Command("TARGET")]
    public class GetTargetCommand : CommandBase
    {
        public IDataTarget Target { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            var target = Target.GetObjectByKeyOrCreate(ctx.Source.Key);

            //target can be empty when using TransitMode = OnlyExitedObjects
            if (target == null)
            {
                ctx.Flow = TransitionFlow.SkipObject;
                return;
            }

            ctx.Target = target;
            TraceColor = ConsoleColor.Magenta;
            ctx.TraceLine($"PIPELINE '{ctx.DataPipeline.Name}' OBJECT, Row {ctx.Source.RowNumber}, Key [{ctx.Source.Key}], IsNew:  {target.IsNew}");
        }
    }
}