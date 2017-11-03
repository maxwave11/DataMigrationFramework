using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;

namespace XQ.DataMigration.Enums
{
    /// <summary>
    /// Determines the level of trace details
    /// </summary>
    public enum TraceLevel {

        Auto = 0,
        /// <summary>
        /// Output information only about <see cref="ObjectSetTransition"/>. All nested nodes will be excluded from logging.
        /// </summary>
        ObjectSet = 1,

        /// <summary>
        /// Output transition info about objects transitions but exclude transitions which are children of <see cref="ObjectTransition"/>'s.
        /// </summary>
        Object = 2,

        /// <summary>
        /// Output full information about transition process (all Transition nodes will be included). 
        /// Warning: This mode will write a lot of information. Use this option only while debugging in order to find some issue in data migration
        /// </summary>
        Verbose = 3
    }
}