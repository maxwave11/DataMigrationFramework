using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Transition which allows to write custom messages to migration trace
    /// </summary>
    public class MessageCommand: ExpressionCommand
    {
        protected  override void ExecuteInternal(ValueTransitContext ctx)
        {
            base.ExecuteInternal(ctx);
            if (ctx.Flow == TransitionFlow.Continue)
                TraceLine(ctx.TransitValue?.ToString(), ctx);
        }
    }
}
