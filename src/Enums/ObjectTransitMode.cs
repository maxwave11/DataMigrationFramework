
namespace DataMigration.Enums
{
    public enum ObjectTransitMode
    {
        /// <summary>
        /// ExecuteInternal both kind of objects (no matter whether they exist in target system). If object with appropriate
        /// key doesn't exist in the target system it will be created and transitted. If object already exists in target
        /// system - it will be fetched from target system, updated by data from source system and saved back to target system.
        /// </summary>
        AllObjects,

        /// <summary>
        /// ExecuteInternal only new objects which are not exist in target system yet. If object already exists in target
        /// system, migrator will not even transit this object. It can be very useful when some kind of objects
        /// takes a lot of time for migration to target system (for exmaple images). By using this option 
        /// you can transit all objects from source system and migrator automatically skips already migrated objects.
        /// Existence of object determined by object migration key <see cref="KeyTransition"/>
        /// </summary>
        OnlyNewObjects,

        /// <summary>
        /// ExecuteInternal only existed objects which are already exist in the target system. In this case 
        /// existed objects will be fetched from target sytem, updated by data from source system and saved back to target system.
        /// No new objects will be created/transitted/saved. 
        /// It can be useful if you want to update already existed objects (in target system) by some data from different sources
        /// in separate migration steps.
        /// Existence of object determined by object migration key <see cref="KeyTransition"/>
        /// </summary>
        OnlyExistedObjects,
    }
}