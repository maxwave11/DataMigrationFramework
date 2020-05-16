using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("TRACE")]
    public class TraceCommand : CommandBase
    {
        public bool Trace { get; set; }
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            ctx.Trace = this.Trace;
        }
        
        public static implicit operator TraceCommand(string expression)
        {
            return new TraceCommand() { Trace = !expression.IsNotEmpty() || bool.Parse(expression)  };
        }
    }
}