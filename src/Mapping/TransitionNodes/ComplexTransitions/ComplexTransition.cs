using System;
using System.Collections.Generic;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions
{
    public abstract class ComplexTransition : TransitionNode
    {
        /// <summary>
        /// List of nested transitions. 
        /// </summary>
        public List<TransitionNode> ChildTransitions { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            ChildTransitions?.ForEach(i => i.Initialize(this));
            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext transitContext)
        {
            if (transitContext == null)
                throw new ArgumentNullException($"{nameof(transitContext)} can't be null in {nameof(ComplexTransition)}");

            return TransitChildren(transitContext);
        }

        protected TransitResult TransitChildren(ValueTransitContext ctx)
        {
            if (ChildTransitions == null)
                return new TransitResult(TransitContinuation.Continue,"Transition is Empty");

            foreach (var childTransition in ChildTransitions)
            {

                if (!childTransition.CanTransit(ctx))
                    continue;

                var childTransitResult = TransitChild(childTransition, ctx);

                if (childTransitResult.Continuation != TransitContinuation.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}");
                    return childTransitResult;
                }
            }

            return new TransitResult(ctx.TransitValue);
        }

        protected virtual TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            var childTransitResult =  childNode.TransitInternal(ctx);
            childTransitResult = EndTransitChild(childTransitResult, ctx);
            return childTransitResult;
        }

        protected virtual TransitResult EndTransitChild(TransitResult result, ValueTransitContext ctx)
        {
            if (result.Continuation == TransitContinuation.SkipUnit)
                return new TransitResult(result.Value);

            return result;
        }
    }
}
