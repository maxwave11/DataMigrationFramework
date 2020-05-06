using System;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;

namespace XQ.DataMigration.MapConfiguration
{
    public class KeyTransition: ComplexTransition<TransitionNode>
    {
        public override void Initialize(TransitionNode parent)
        {
            Trace = MapConfig.Current.TraceKeyTransition;
            Color = ConsoleColor.Blue;
            base.Initialize(parent);
        }

        public string GetKeyForObject(IValuesObject sourceObject)
        {
            var ctx = new ValueTransitContext(sourceObject, null, sourceObject);
            var transitResult = this.TransitCore(ctx);

            return transitResult.Value?.ToString();
        }

        // protected virtual string GetKeyFromSource(IValuesObject sourceObject)
        // {
        //     if (!sourceObject.Key.IsEmpty())
        //         return sourceObject.Key;
        //
        //     var ctx = new ValueTransitContext(sourceObject, null, sourceObject);
        //     var transitResult = SourceKeyTransition.TransitCore(ctx);
        //
        //     if (transitResult.Flow == TransitionFlow.Continue)
        //         sourceObject.Key = transitResult.Value?.ToString();
        //
        //     if (transitResult.Flow == TransitionFlow.RaiseError || transitResult.Flow == TransitionFlow.Stop)
        //     {
        //         TraceLine($"Transition stopped on { Name }");
        //         throw new Exception("Can't transit source key ");
        //     }
        //
        //     return sourceObject.Key;
        // }
        
        public static implicit operator KeyTransition(string expression)
        {
            return new KeyTransition() { new ReadTransitUnit() { Expression = expression } };
        }
        
        
    }


}
