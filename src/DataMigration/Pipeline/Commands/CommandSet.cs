using System;
using System.Collections;
using System.Collections.Generic;
using DataMigration.Enums;

namespace DataMigration.Pipeline.Commands
{
    [Yaml("TRANSIT")]
    public class CommandSet<T> : CommandBase, IList<T>  where T : CommandBase
    {
        public IList<T> Commands { get; set; } = new List<T>();

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            foreach (var childTransition in Commands)
            {
                TransitChild(childTransition, ctx);

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    ctx.TraceLine($"Breaking {this.GetType().Name}");
                    break;
                }
            }
        }

        protected virtual void TransitChild(T childCommand, ValueTransitContext ctx)
        {
            ctx.Execute(childCommand);
        }

        // public static implicit operator CommandSet<T>(string expression)
        // {
        //     return new CommandSet<T>() { Commands = new List<T>() { (T)expression }};
        // }
        
        #region IList

        public void Add(T item)
        {
            Commands.Add(item);
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

        public void Add(CommandBase[] item)
        {
            throw new NotImplementedException();
        }
    }
}
