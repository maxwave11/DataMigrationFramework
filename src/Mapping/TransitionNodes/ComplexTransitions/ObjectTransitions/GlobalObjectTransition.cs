using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    public class GlobalObjectTransition: ObjectTransition
    {
        public static IValuesObject GlobalObject { get; private set; } = new ValuesObject();

        public override TransitResult Transit(ValueTransitContext transitContext)
        {
            var ctx = new ValueTransitContext(GlobalObject, GlobalObject, null, this);
            return base.Transit(ctx);
        }

        protected override void Validate()
        {
        }
    }
}