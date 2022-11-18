using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{
    [Yaml("TRACE")]
    public class TraceCommand : CommandBase
    {
        public bool Trace { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            ctx.Trace = this.Trace;
        }
        
        public static implicit operator TraceCommand(string expression)
        {
            return new TraceCommand() { Trace = !expression.IsNotEmpty() || bool.Parse(expression)  };
        }
    }
}