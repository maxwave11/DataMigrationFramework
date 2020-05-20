using System.Linq;
using XQ.DataMigration.Pipeline.Expressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{    
    [Command("CONCAT")]
    public class ConcatCommand : CommandSet<GetCommand>
    {
        string _key = "";
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            _key = "";
            base.ExecuteInternal(ctx);
            ctx.SetCurrentValue(_key.TrimEnd('/'));
        }
        protected override void TransitChild(GetCommand childTransition, ValueTransitContext ctx)
        {
            base.TransitChild(childTransition, ctx);
            _key += ctx.TransitValue + "/";
        }

        public override string ToString()
        {
            return $"[{ Pipeline.Select(i => i.ToString()).Join() }]";
        }
    }
}