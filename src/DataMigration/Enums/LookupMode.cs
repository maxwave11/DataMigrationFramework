namespace DataMigration.Enums
{
    public enum LookupMode
    {
        /// <summary>
        /// Find single object by key from data source (default mode). Lookup will throw an error if 
        /// more than one object will found.
        /// </summary>
        Single,
        /// <summary>
        /// Find first object by key from data source (when data source has multiple objects with same key)
        /// </summary>
        First,
        /// <summary>
        /// Use this mode to find all objects by search key. Lookup command will return found object's collection.
        /// </summary>
        All
    }
}