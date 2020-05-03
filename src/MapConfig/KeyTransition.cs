using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.MapConfig
{
    public class KeyTransition: TransitValueCommand
    {
        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Blue;
            base.Initialize(parent);
        }

        public string GetKeyForObject(IValuesObject sourceObject)
        {
            var ctx = new ValueTransitContext(sourceObject, null, sourceObject);
            var transitResult = this.TransitCore(ctx);

            if (transitResult.Continuation == TransitContinuation.RaiseError || transitResult.Continuation == TransitContinuation.Stop)
            {
                TraceLine($"Transition stopped on { Name }");
                throw new Exception("Can't transit object key ");
            }

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
        //     if (transitResult.Continuation == TransitContinuation.Continue)
        //         sourceObject.Key = transitResult.Value?.ToString();
        //
        //     if (transitResult.Continuation == TransitContinuation.RaiseError || transitResult.Continuation == TransitContinuation.Stop)
        //     {
        //         TraceLine($"Transition stopped on { Name }");
        //         throw new Exception("Can't transit source key ");
        //     }
        //
        //     return sourceObject.Key;
        // }
        
        public static implicit operator KeyTransition(string expression)
        {
            return new KeyTransition() { From = expression };
        }
    }


}
