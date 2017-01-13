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
        public LogVerbosity LogVerbosity { get; set; }

        [XmlAttribute]
        public ConsoleColor ConsoleColor { get; set; }  = ConsoleColor.White;

        public LogVerbosity ActualLogVerbosity => LogVerbosity == LogVerbosity.Auto ? Parent?.ActualLogVerbosity ?? LogVerbosity : LogVerbosity;

        [XmlIgnore]
        public TransitionNode Parent { get; private set; }

        public virtual void Initialize(TransitionNode parent)
        {
            Parent = parent;
            GetChildren()?.ForEach(i => i.Initialize(this));
        }

        public abstract List<TransitionNode> GetChildren();  

        public string TreeInfo()
        {
            var childrenInfo = GetChildren() != null
                ? "\n" + String.Join("\n", GetChildren().Select(i => i.TreeInfo()))
                : "";
            return GetIndent() + GetInfo() + childrenInfo;
        }

        protected string GetIndent(int additionSpaceCount = 0)
        {
            string indent = "";
            TransitionNode nextParent = Parent;
            while (nextParent != null)
            {
                nextParent = nextParent.Parent;
                indent+="  ";
            }
            return indent + (additionSpaceCount>0 ? new string(' ', additionSpaceCount):"");
        }

        public virtual string GetInfo()
        {
            return $"({this.GetType().Name})" + 
                (Name.IsNotEmpty() ? $"\n{GetIndent(5)}Name: {Name}" : "");
        }
    }
}