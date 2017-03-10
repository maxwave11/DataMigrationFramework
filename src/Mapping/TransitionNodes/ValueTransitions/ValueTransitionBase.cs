using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;

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
            Migrator.Current.Tracer.TraceValueTransitionStart(this,ctx);
            TransitContinuation continuation = TransitContinuation.Continue;
            //at first start process child transitions
            if (ChildTransitions != null)
            {
                foreach (var childTransition in ChildTransitions)
                {
                    var result = childTransition.TransitValueInternal(ctx);
                    continuation = result.Continuation;

                    if (continuation == TransitContinuation.SkipUnit)
                    {
                        continuation = TransitContinuation.Continue;
                        break;
                    }

                    if (continuation != TransitContinuation.Continue)
                        break;
                }
            }

            //process own transition just after childrens
            if (continuation == TransitContinuation.Continue)
                continuation = HandleValueTransition(ctx);

            Migrator.Current.Tracer.TraceValueTransitionEnd(this, ctx);
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
                message = result.Message;
            }
            catch (Exception ex)
            {
                continuation = this.OnError;
                Migrator.Current.Tracer.TraceText(ex.ToString(),this, ConsoleColor.Yellow);
            }

            if (continuation == TransitContinuation.RaiseError)
            {
                message = $"Transition stopped, message: {message}";
                continuation = Migrator.Current.Tracer.TraceError(message, this, ctx);
            }

            ctx.SetCurrentValue(this.Name, resultValue);
            return continuation;
        }
    }
}
