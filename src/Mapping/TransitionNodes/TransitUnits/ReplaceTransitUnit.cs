using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReplaceTransitUnit : ValueTransitionBase
    {
        [XmlAttribute]
        public string ReplaceRules { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (ReplaceRules.IsNotEmpty())
            {
                if (ChildTransitions?.Any() == true)
                    throw new Exception(nameof(ReplaceTransitUnit) + "not allows not empty Rules property while having child rules collection");

                var rules = ReplaceRules.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                ChildTransitions = new List<ValueTransitionBase>();
                ChildTransitions.AddRange(rules.Select(i => new ReplaceRule { Rule = i}));
            }

            base.Initialize(parent);
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "ReplaceRules: " + ReplaceRules;
        }
    }
}