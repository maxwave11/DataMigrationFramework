using DataMigration.Enums;
using DataMigration.Pipeline.Expressions;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Transition which allows to write custom messages to migration trace
    /// </summary>
    [Command("MSG")]
    public class MessageCommand: CommandBase
    {
        public MigrationExpression<string> Message { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            ctx.TraceLine(Message.Evaluate(ctx));
        }

        public static implicit operator MessageCommand(string expression)
        {
            return new MessageCommand() { Message = expression };
        }
    }
}
