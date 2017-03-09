using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfig;

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

        public virtual void Initialize(TransitionNode parent)
        {
            Parent = parent;
            GetChildren()?.ForEach(i => i.Initialize(this));
        }

        public abstract List<TransitionNode> GetChildren();  

        public override string ToString()
        {
            return $"({Name ?? this.GetType().Name})";
        }
    }
}