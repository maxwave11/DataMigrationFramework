namespace DataMigration.Enums
{
    /// <summary>
    /// Determines migration engine behaviour
    /// </summary>
    public enum PipelineFlowControl
    {
        Continue,
        Stop,
        SkipValue,
        SkipObject,
        Debug
    }
}