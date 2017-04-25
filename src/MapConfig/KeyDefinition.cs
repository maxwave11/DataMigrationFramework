using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.MapConfig
{
    /// <summary>
    /// Element defines logic by expressions or transitions for evaluating unique migration keys for source and target objects
    /// by existed data of this objects. Matching source and target objects based on this migration key and it's very important to
    /// define key evaluation logic. Since source and target objects are of different types (becaus source and target systems are different), 
    /// almost always you have to define key evaluation logic for source object and for target object respectively. 
    /// Migration key evaluated for source and target objects must be equal!
    /// </summary>
    public class KeyDefinition: ComplexTransition
    {
        /// <summary>
        /// Defines an expression which must evaluate a unique migration key for source object of transition
        /// </summary>
        [XmlAttribute]
        public string SourceKey { get; set; }

        /// <summary>
        /// Defines an expression which must evaluate a unique migration key for target object of transition
        /// </summary>
        [XmlAttribute]
        public string TargetKey { get; set; }

        /// <summary>
        /// Defines a set of transitions which must get a unique migration key for source object of transition. Use this attribute 
        /// if key definition is complex and SourceKey attribute is not enough
        /// </summary>
        [XmlElement]
        public ValueTransition SourceKeyTransition { get; set; }

        /// <summary>
        /// Defines a set of transitions which must get a unique migration key for target object of transition. Use this attribute 
        /// if key definition is complex and SourceKey attribute is not enough
        /// </summary>
        [XmlElement]
        public ValueTransition TargetKeyTransition { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (SourceKey.IsEmpty() && SourceKeyTransition == null)
                throw new Exception($"{nameof(SourceKey)} or nested {nameof(SourceKeyTransition)}  is required for {nameof(KeyDefinition)} element");

            if (TargetKey.IsEmpty() && TargetKeyTransition == null)
                throw new Exception($"{nameof(TargetKey)} or nested {nameof(TargetKeyTransition)}  is required for {nameof(KeyDefinition)} element");

            if (SourceKey.IsNotEmpty())
            {
                if (SourceKeyTransition != null)
                    throw new Exception($"Setting {nameof(SourceKey)} and nested {nameof(SourceKeyTransition)} are not allowed at the same time");

                SourceKeyTransition = new ValueTransition { From = SourceKey};
            }
            if (TargetKey.IsNotEmpty())
            {
                if (TargetKeyTransition != null)
                    throw new Exception($"Setting {nameof(TargetKey)} and nested {nameof(TargetKeyTransition)} are not allowed at the same time");

                TargetKeyTransition = new ValueTransition { From = TargetKey};
            }
            
            SourceKeyTransition.Initialize(parent);
            TargetKeyTransition.Initialize(parent);

            SourceKeyTransition.Name = "SourceKeyTransition";
            TargetKeyTransition.Name = "TargetKeyTransition";

            //mark key transition by special colors
            SetColorRecursive(SourceKeyTransition, ConsoleColor.Blue);
            SetColorRecursive(TargetKeyTransition, ConsoleColor.Blue);
        }

        private void SetColorRecursive(TransitionNode node, ConsoleColor color)
        {
            node.Color = color;
            var children = (node as ComplexTransition)?.ChildTransitions;
            if (children?.Any()  != true)
                return;

            foreach (var childNode in children)
            {
                SetColorRecursive(childNode, color);
            }
        }
    }
}
