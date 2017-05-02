﻿using System;
using System.Collections.Generic;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

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

            var continuation = TransitChildren(transitContext);
            return new TransitResult(continuation, transitContext.TransitValue);
        }

        protected TransitContinuation TransitChildren(ValueTransitContext ctx)
        {
            var continuation = TransitContinuation.Continue;

            if (ChildTransitions == null)
                return continuation;

            foreach (var childTransition in ChildTransitions)
            {
                var result = childTransition.TransitInternal(ctx);
                continuation = result.Continuation;

                if (continuation == TransitContinuation.SkipUnit)
                {
                    continuation = TransitContinuation.Continue;
                    break;
                }

                if (continuation != TransitContinuation.Continue)
                    break;
            }

            return continuation;
        }
    }
}