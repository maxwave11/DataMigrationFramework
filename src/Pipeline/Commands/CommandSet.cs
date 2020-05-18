using System;
using System.Collections;
using System.Collections.Generic;
using XQ.DataMigration.Enums;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("TRANSIT")]
    public class CommandSet<T> : CommandBase, IList<T>  where T : CommandBase
    {
        public List<T> Pipeline { get; set; } = new List<T>();
        
        protected  override void ExecuteInternal(ValueTransitContext ctx)
        {
            foreach (var childTransition in Pipeline)
            {
                TransitChild(childTransition, ctx);

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {this.GetType().Name}", ctx);
                    break;
                }
            }
        }

        protected virtual void TransitChild(T childTransition, ValueTransitContext ctx)
        {
            childTransition.Execute(ctx);
        }

        public override string ToString()
        {
            return $"Pipeline: { Pipeline.Count }";
        }
        
        public static implicit operator CommandSet<T>(string expression)
        {
            return new CommandSet<T>() { Pipeline = new List<T>() { (T)expression }};
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
