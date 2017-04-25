using XQ.DataMigration.Data;

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

        protected override void Validate()
        {
        }
    }
}