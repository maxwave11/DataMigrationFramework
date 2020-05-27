using System;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline.Commands;

namespace XQ.DataMigration.Pipeline
{
    [Command("TARGET")]
    public class GetTargetCommand : CommandBase
    {
        public IDataTarget Target { get; set; }

        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            var target = Target.GetObjectByKeyOrCreate(ctx.Source.Key);

            if (target == null)
            {
                ctx.Flow = TransitionFlow.SkipObject;
                return;
            }

            ctx.Target = target;
            TraceColor = ConsoleColor.Magenta;
            TraceLine($"PIPELINE '{ctx.DataPipeline.Name}' OBJECT, Row {ctx.Source.RowNumber}, Key [{ctx.Source.Key}], IsNew:  {target.IsNew}", ctx);
        }
    }
}