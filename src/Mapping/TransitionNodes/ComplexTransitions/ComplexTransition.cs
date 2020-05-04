using System;
using System.Collections;
using System.Collections.Generic;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions
{
    public abstract class ComplexTransition : TransitionNode, IList<TransitionNode>
    {
        /// <summary>
        /// List of nested transitions. 
        /// NOTE: generic parameter should be a class (not interface) since it will be not deserialized from XML
        /// </summary>
        public List<TransitionNode> Pipeline { get; set; } = new List<TransitionNode>();

        public override void Initialize(TransitionNode parent)
        {
            Pipeline.ForEach(i => i.Initialize(this));
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
            try
            {
                Migrator.Current.Tracer.Indent();
                
                foreach (var childTransition in Pipeline)
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
            }
            finally
            {
                Migrator.Current.Tracer.IndentBack();
            }

            return new TransitResult(ctx.TransitValue);
        }

        protected virtual TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            var childTransitResult =  childNode.TransitCore(ctx);
            childTransitResult = EndTransitChild(childTransitResult, ctx);
            return childTransitResult;
        }

        protected virtual TransitResult EndTransitChild(TransitResult result, ValueTransitContext ctx)
        {
            if (result.Continuation == TransitContinuation.SkipUnit)
                return new TransitResult(result.Value);

            return result;
        }

        public IEnumerator<TransitionNode> GetEnumerator()
        {
            throw new NotImplementedException();
        }

    

        public void Add(TransitionNode item)
        {
            Pipeline.Add(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TransitionNode item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TransitionNode[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TransitionNode item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(TransitionNode item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TransitionNode item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public TransitionNode this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
