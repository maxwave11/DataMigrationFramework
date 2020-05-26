using System.Linq;
using XQ.DataMigration.Pipeline.Expressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{    
    [Command("CONCAT")]
    public class ConcatCommand : CommandSet<ExpressionCommand<object>>
    {
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            string result = Commands.Select(i => i.Execute(ctx)?.ToString()).Join("/");
            ctx.SetCurrentValue(result);
        }
        public override string GetParametersInfo() => $"[{ Commands.Select(i => i.GetParametersInfo()).Join() }]";
    }
}