using System.Linq;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ConcatReadTransition : ComplexTransition<ReadTransitUnit>
    {
        string _key = "";
        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            _key = "";
            base.TransitInternal(ctx);
            return new TransitResult(_key.TrimEnd('/'));
        }
        protected override TransitResult TransitChild(ReadTransitUnit childTransition, ValueTransitContext ctx)
        {
            var result =  base.TransitChild(childTransition, ctx);
            _key += result.Value + "/";
            return new TransitResult(null);
        }

        public static implicit operator ConcatReadTransition(string expression)
        {
            var retVal =  new ConcatReadTransition() { expression };
            return retVal;
        }

        public override string ToString()
        {
            return $"[{ Pipeline.Select(i=>i.ToString()).Join() }]";
        }
    }   
    
   
}