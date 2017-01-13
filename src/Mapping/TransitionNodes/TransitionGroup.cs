using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    public class TransitionGroup : TransitionNode
    {
        public List<ObjectTransition> ObjectTransitions { get; set; }

        public void Run()
        {
            foreach (var objTransition in ObjectTransitions)
            {
                if (!objTransition.Enabled)
                    continue;

                if (objTransition is GlobalObjectTransition)
                {
                    objTransition.TransitObject(null);
                }
                else
                {
                    objTransition.TransitAllObjects();
                }
            }
        }

        public override List<TransitionNode> GetChildren()
        {
            return ObjectTransitions.Cast<TransitionNode>().ToList();
        }
    }
}