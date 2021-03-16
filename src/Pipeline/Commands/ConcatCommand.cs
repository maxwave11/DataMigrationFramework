using System.Linq;
using DataMigration.Pipeline.Expressions;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{    
    [Command("CONCAT")]
    public class ConcatCommand : CommandSet<ExpressionCommand<object>>
    {
        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            string result = Commands.Select(i => ctx.Execute(i)?.ToString()).Join("/");
            ctx.SetCurrentValue(result);
        }
        public override string GetParametersInfo() => $"[{ Commands.Select(i => i.GetParametersInfo()).Join() }]";
    }
}