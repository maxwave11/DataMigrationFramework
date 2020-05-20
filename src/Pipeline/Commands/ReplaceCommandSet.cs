using System;
using System.Linq;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("REPLACE")]
    public class ReplaceCommandSet : CommandSet<ReplaceStepCommand>
    {
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            foreach (var childTransition in Commands)
            {
                childTransition.Execute(ctx);

                if (ctx.Flow == TransitionFlow.SkipValue)
                {
                    //if ReplaceUnit returned SkipValue then need to stop ONLY replacing sequence (hack, need to refactor to do
                    //it in more convenient way
                    ctx.Flow = TransitionFlow.Continue;
                    break;
                }

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}", ctx);
                    break;
                }
            }
        }


        public static implicit operator ReplaceCommandSet(string expression)
        {
            if (expression.IsEmpty())
                throw new Exception("Replace expression cant be empty");

            var rules = expression.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return new ReplaceCommandSet() { Commands = rules.Select(i=> (ReplaceStepCommand)i).ToList()};
        }
    }
}