namespace DataMigration.Enums
{
    /// <summary>
    /// Determines migration engine behaviour
    /// </summary>
    public enum TransitionFlow
    {
        Continue,
        RiseError,
        SkipValue,
        SkipObject,
        Debug
    }
}