using System;
using System.Linq;
using DataMigration.Enums;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Command allows to implement complex string replacing logic
    /// </summary>
    [Command("REPLACE")]
    public class ReplaceCommandSet : CommandSet<ReplaceStepCommand>
    {
        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            foreach (var childTransition in Commands)
            {
                ctx.Execute(childTransition);

                if (ctx.Flow == TransitionFlow.SkipValue)
                {
                    //if ReplaceUnit returned SkipValue then need to stop ONLY replacing sequence (hack, need to refactor to do
                    //it in more convenient way
                    ctx.Flow = TransitionFlow.Continue;
                    break;
                }

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    ctx.TraceLine($"Breaking {this.GetType().Name}");
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