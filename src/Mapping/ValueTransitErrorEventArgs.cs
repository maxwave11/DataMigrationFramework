using System;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;

namespace XQ.DataMigration.Mapping
{
    public class ValueTransitErrorEventArgs
    {
        public ValueTransitionBase ValueTransition { get; private set; }
        public ValueTransitContext Context { get; private set; }
        public bool Continue { get; set; }

        public ValueTransitErrorEventArgs(ValueTransitionBase valueTransition, ValueTransitContext context)
        {
            ValueTransition = valueTransition;
            Context = context;
        }

        public override string ToString()
        {
            string errorMsg = "Error description";
            errorMsg += "Errornous value transition: \n" + ValueTransition.TreeInfo();
            errorMsg += "\n";
            TransitionNode rootValueTransition = ValueTransition;

            while (rootValueTransition.Parent is ValueTransitionBase)
            {
                rootValueTransition = rootValueTransition.Parent;
            }

            errorMsg += "============ TRACE ========== ";
            foreach (var traseEntry in Context.TraceEntries)
            {
                errorMsg += traseEntry.Mesage;
                //TODO
                //TransitLogger.LogInfo(traseEntry.Mesage, traseEntry.Color);
            }

            errorMsg += "\n============== Current Transition info=============== \n" + rootValueTransition.TreeInfo();

            try
            {
                errorMsg += "Context: \n" + Context.GetInfo();
            }
            catch (Exception ex)
            {
                throw new Exception("Exception while getting context info: " + ex);
            }


            return errorMsg;
        }
    }
}