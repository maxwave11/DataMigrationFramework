using XQ.DataMigration.MapConfig;

namespace XQ.DataMigration.Enums
{
    public enum ObjectTransitMode
    {
        /// <summary>
        /// Transit both kind of objects (no matter whether they exist in target system)
        /// </summary>
        AllObjects,
        /// <summary>
        /// Transit only new objects which are not exist in target system yet. If object already exists in target
        /// system, migrator will not even transit this object. It can be very useful when some kind of objects
        /// takes a lot of time for migration to target system (for exmaple images). By using this optin 
        /// you can transit all objects from source system and migrator automatically skips already migrated objects.
        /// Existence of object determined by object migration key <see cref="KeyTransition"/>
        /// </summary>
        OnlyNewObjects,

        /// <summary>
        /// Transit only existed objects which are already exist in target system. In this case 
        /// existed objects will be just updated from source system. No new objects will be transitted/saved.
        /// Existence of object determined by object migration key <see cref="KeyTransition"/>
        /// </summary>
        OnlyExistedObjects,
    }
}