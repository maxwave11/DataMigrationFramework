using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    /// <summary>
    /// Base class for any transition element in Map configuration
    /// </summary>
    public abstract class TransitionNode
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool Enabled { get; set; } = true;

        [XmlAttribute]
        public TraceMode Trace { get; set; }

        [XmlAttribute]
        public string TraceMessage { get; set; }

        [XmlAttribute]
        public ConsoleColor Color { get; set; }  = ConsoleColor.White;

        internal TraceMode ActualTrace => Trace == TraceMode.Auto ? Parent?.ActualTrace ?? Trace : Trace;

        [XmlIgnore]
        public TransitionNode Parent { get; private set; }

        private string _indent = null;
        public virtual void Initialize(TransitionNode parent)
        {
            Parent = parent;
            GetChildren()?.ForEach(i => i.Initialize(this));
        }

        public abstract List<TransitionNode> GetChildren();  

        protected string GetIndent(int additionSpaceCount = 0)
        {
            if (_indent == null)
            {
                _indent = "";
                TransitionNode nextParent = Parent;
                while (nextParent != null)
                {
                    nextParent = nextParent.Parent;
                    _indent += "  ";
                }
            }

            return _indent + (additionSpaceCount > 0 ? new string(' ', additionSpaceCount):"");
        }

        public override string ToString()
        {
            return $"({Name ?? this.GetType().Name})";
        }
    }
}