using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions
{
    public abstract class ValueTransitionBase : TransitionNode
    {
        public List<ValueTransitionBase> ChildTransitions { get; set; }

        [XmlAttribute]
        public TransitContinuation OnError { get; set; } = TransitContinuation.RaiseError;

        [XmlAttribute]
        public TransitContinuation OnEmpty { get; set; } = TransitContinuation.Continue;

        internal Expressions.ExpressionEvaluator ExpressionEvaluator { get; } = new Expressions.ExpressionEvaluator();

        public ObjectTransition ObjectTransition => (Parent as ObjectTransition) ?? (Parent as ValueTransitionBase)?.ObjectTransition;

        protected virtual object TransitValueInternal(ValueTransitContext ctx)
        {
            return ctx.TransitValue;
        }

        public override List<TransitionNode> GetChildren()
        {
            return ChildTransitions?.Cast<TransitionNode>().ToList();
        }

        public virtual TransitResult TransitValue(ValueTransitContext ctx)
        {
            TraceTransitionStart(ctx);
            Migrator.Current.RaiseTransitValueStarted(this);
            TransitContinuation continuation;
            if (ChildTransitions != null)
            {
                foreach (var childTransition in ChildTransitions)
                {
                    //TODO
                    //if (!writeToTarget && transitUnit is WriteTransitUnit)
                    //    continue;
                    continuation = HandleValueTransition(childTransition.TransitValue, childTransition, ctx);

                    if (continuation == TransitContinuation.SkipUnit)
                        return new TransitResult(TransitContinuation.Continue, ctx.TransitValue);

                    if (continuation != TransitContinuation.Continue)
                        return new TransitResult(continuation, ctx.TransitValue);
                }
            }

            continuation = HandleValueTransition(ctx2 =>
            {
                return new TransitResult(TransitContinuation.Continue, TransitValueInternal(ctx2));
            }, this, ctx);

            TraceTransitionEnd(ctx);
            return new TransitResult(continuation, ctx.TransitValue);
        }

        private static TransitContinuation HandleValueTransition(Func<ValueTransitContext, TransitResult> transitMethod, ValueTransitionBase transition, ValueTransitContext ctx)
        {
            object resultValue = null;
            TransitContinuation continuation;
            string message = "";
            try
            {
                var result = transitMethod(ctx);
                resultValue = result.Value;
                continuation = result.Continuation;
                ctx.SetCurrentValue(transition.Name, resultValue);
            }
            catch (Exception ex)
            {
                continuation = transition.OnError;
                message = $"Error occured while transitting: " + ex;
            }

            if (resultValue == null || resultValue.ToString().IsEmpty())
            {
                if (transition.OnEmpty != TransitContinuation.Continue)
                {
                    message = "Value transition interupped because of empty value of transition " + transition.Name;
                    continuation = transition.OnEmpty;
                }
            }

            if (continuation == TransitContinuation.RaiseError)
            {
                message = $"Transition stopped on {transition.Name}, message: {message}, info: \n{transition.TreeInfo()}";
                TransitLogger.Log(message);
                continuation = Migrator.Current.InvokeOnTransitError(transition, ctx);
            }

            return continuation;
        }

        protected void Trace(string traceMessage, ValueTransitContext ctx)
        {
            if (ActualVerbose == Verbosity.Verbose)
                TransitLogger.Log(GetIndent() + traceMessage, ConsoleColor);

            ctx.AddTraceEntry(GetIndent() + traceMessage, ConsoleColor);
        }

        private void TraceTransitionStart(ValueTransitContext ctx)
        {
            var traceMsg =
                $"> {GetInfo()}\n{GetIndent(5)}Input: ({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            Trace(traceMsg, ctx);
        }

        private void TraceTransitionEnd(ValueTransitContext ctx)
        {
            var traceMsg = $"< =({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            Trace(traceMsg, ctx);
        }
    }
}
