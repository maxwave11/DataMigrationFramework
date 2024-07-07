namespace DataMigrationCore.Enums
{
    /// <summary>
    /// Determines migration pipeline behaviour
    /// </summary>
    public enum PipelineFlowControl
    {
        Continue,
        Stop,
        SkipPipe,
        SkipObject,
        Debug
    }
}
