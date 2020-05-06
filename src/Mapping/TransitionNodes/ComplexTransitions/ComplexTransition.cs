using System;
using System.Collections;
using System.Collections.Generic;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions
{
    public class ComplexTransition<T> : TransitionNode, IList<T>  where T : TransitionNode
    {
        public List<T> Pipeline { get; set; } = new List<T>();

        public override void Initialize(TransitionNode parent)
        {
            Pipeline.ForEach(i => i.Initialize(this));
            base.Initialize(parent);
        }

        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            foreach (var childTransition in Pipeline)
            {
                var childTransitResult = TransitChild(childTransition, ctx);

                if (childTransitResult.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}", ctx);
                    return childTransitResult;
                }
            }

            return new TransitResult(ctx.TransitValue);
        }

        protected virtual TransitResult TransitChild(T childTransition, ValueTransitContext ctx)
        {
            return childTransition.Transit(ctx);
        }

        #region IList

        public void Add(T item)
        {
            Pipeline.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        
        #endregion
    }
}
