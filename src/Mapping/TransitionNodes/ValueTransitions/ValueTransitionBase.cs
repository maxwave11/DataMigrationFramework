using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions
{
    public abstract class ValueTransitionBase : TransitionNode
    {
        /// <summary>
        /// List of nested transitions. 
        /// </summary>
        public List<ValueTransitionBase> ChildTransitions { get; set; }

        /// <summary>
        /// Specify what to do if some error occured while current transition processing
        /// </summary>
        [XmlAttribute]
        public TransitContinuation OnError { get; set; } = TransitContinuation.RaiseError;

        /// <summary>
        /// Specify what to do if current transition returns empty string or null
        /// </summary>
        [XmlAttribute]
        public TransitContinuation OnEmpty { get; set; } = TransitContinuation.Continue;

        /// <summary>
        /// Error message which end user should see when this transition returns empty string or null. It can be 
        /// as Migration expression like  - "Asset with ID {SRC[asset_id]} was not found in target system"
        /// </summary>
        [XmlAttribute]
        public string OnEmptyMessage { get; set; }

        internal ObjectTransition ObjectTransition => (Parent as ObjectTransition) ?? (Parent as ValueTransitionBase)?.ObjectTransition;

        internal Expressions.ExpressionEvaluator ExpressionEvaluator { get; } = new Expressions.ExpressionEvaluator();

        public virtual TransitResult TransitValue(ValueTransitContext ctx)
        {
            return new TransitResult(TransitContinuation.Continue, ctx.TransitValue);
        }

        public override List<TransitionNode> GetChildren()
        {
            return ChildTransitions?.Cast<TransitionNode>().ToList();
        }

        internal TransitResult TransitValueInternal(ValueTransitContext ctx)
        {
            TraceTransitionStart(ctx);
            Migrator.Current.RaiseTransitValueStarted(this);
            TransitContinuation continuation;
            //at first start process child transitions
            if (ChildTransitions != null)
            {
                foreach (var childTransition in ChildTransitions)
                {
                    var result = childTransition.TransitValueInternal(ctx);
                    continuation = result.Continuation;

                    if (continuation == TransitContinuation.SkipUnit)
                        return new TransitResult(TransitContinuation.Continue, ctx.TransitValue);

                    if (continuation != TransitContinuation.Continue)
                        return new TransitResult(continuation, ctx.TransitValue);
                }
            }

            //process own transition just after childrens
            continuation = HandleValueTransition(ctx);

            TraceTransitionEnd(ctx);
            return new TransitResult(continuation, ctx.TransitValue);
        }

        private TransitContinuation HandleValueTransition(ValueTransitContext ctx)
        {
            object resultValue = null;
            TransitContinuation continuation;
            string message = "";
            try
            {
                var result = TransitValue(ctx);
                resultValue = result.Value;
                continuation = result.Continuation;
            }
            catch (Exception ex)
            {
                continuation = this.OnError;
                TraceErrorLine(ex.ToString(), ctx);
            }

            if (resultValue == null || resultValue.ToString().IsEmpty())
            {
                if (this.OnEmpty != TransitContinuation.Continue)
                {
                    message = "Value transition interuppted because of empty value of transition " + this.Name;
                    if (this.OnEmptyMessage.IsNotEmpty())
                    {
                        message = this.ExpressionEvaluator.EvaluateString(this.OnEmptyMessage + "{}", ctx);
                    }
                    continuation = this.OnEmpty;
                }
            }

            if (continuation == TransitContinuation.RaiseError)
            {
                message = $"Transition stopped, message: {message}";
                TraceErrorLine(message, ctx);
                continuation = Migrator.Current.InvokeOnTransitError(this, ctx);
            }

            ctx.SetCurrentValue(this.Name, resultValue);

            return continuation;
        }

        protected void TraceTransitionMessage(string msg, ValueTransitContext ctx, ConsoleColor color)
        {
            TraceLine(GetIndent(5) + msg, ctx, color);
        }

        protected void TraceTransitionMessage(string msg, ValueTransitContext ctx)
        {
            TraceTransitionMessage(msg, ctx, ConsoleColor);
        }

        private void TraceTransitionStart(ValueTransitContext ctx)
        {
            var traceMsg =
                $"> {this.ToString()}\n{GetIndent(5)}Input: ({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            TraceLine(traceMsg, ctx, ConsoleColor);
        }

        private void TraceTransitionEnd(ValueTransitContext ctx)
        {
            var traceMsg = $"< =({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            TraceLine(traceMsg, ctx, ConsoleColor);
        }

        private void TraceLine(string traceMessage, ValueTransitContext ctx, ConsoleColor color)
        {
            if (ActualTrace == TraceMode.True)
                Migrator.Current.InvokeTrace(GetIndent() + traceMessage, color);

            //Debug.WriteLine(GetIndent() + traceMessage);
            ctx.AddTraceEntry(GetIndent() + traceMessage, color);
        }

        private void TraceErrorLine(string traceMessage, ValueTransitContext ctx)
        {
            Migrator.Current.InvokeTrace(GetIndent() + traceMessage, ConsoleColor.Red);
            ctx.AddTraceEntry(GetIndent() + traceMessage, ConsoleColor.Red);
        }
    }
}
