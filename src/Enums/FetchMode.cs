using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;


namespace XQ.DataMigration.Enums
{
    /// <summary>
    /// Determines how <see cref="TransitDataCommand"/> will fetch source objects
    /// </summary>
    public enum FetchMode
    {
        /// <summary>
        /// Means that <see cref="TransitDataCommand"/> will read source objects directly from <see cref="ISourceProvider"/>
        /// </summary>
        SourceProvider,

        /// <summary>
        /// Means that <see cref="TransitDataCommand"/> will read source objects from Source obect of <see cref="ValueTransitContext"/>
        /// </summary>
        SourceObject
    }
}