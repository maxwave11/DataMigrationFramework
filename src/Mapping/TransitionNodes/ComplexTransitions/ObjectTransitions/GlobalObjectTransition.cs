using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    public class GlobalObjectTransition: ObjectTransition
    {
        public static IValuesObject GlobalObject { get; private set; } = new ValuesObject();

        protected override string GetKeyFromSource(IValuesObject sourceObject)
        {
            return "global_dummy_key";
        }

        protected override IValuesObject GetTargetObject(string key)
        {
            return GlobalObject;
        }

        public override TransitResult Transit(ValueTransitContext transitContext)
        {
            var ctx = new ValueTransitContext(null, GlobalObject, null, this);
            var continuation = TransitChildren(ctx);
            return new TransitResult(continuation, ctx.TransitValue);
        }

        protected override void Validate()
        {
        }
    }
}