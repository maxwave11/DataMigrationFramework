using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Transition which allows to write custom messages to migration trace
    /// </summary>
    public class MessageCommand: CommandBase
    {
        public string Message { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (ctx.Flow == TransitionFlow.Continue)
                ctx.TraceLine(Message);
        }
    }
}
