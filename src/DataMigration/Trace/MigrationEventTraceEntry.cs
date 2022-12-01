using DataMigration.Enums;
using DataMigration.Pipeline;

namespace DataMigration.Trace;

public class MigrationEventTraceEntry
{
    public uint RowNumber { get; set; }
    public string DataSetName { get; set; }

    public MigrationEvent EventType { get; }

    public string Query { get; set; }

    public string ObjectKey { get; set; }

    public string Message { get; }

    public MigrationEventTraceEntry(MigrationEvent eventType, ValueTransitContext ctx, string message)
    {
        EventType = eventType;
        Message = message;
        ObjectKey = ctx.Source.Key;
        //DataSetName = ctx.DataPipeline?.Name;
        //Query = ctx.DataPipeline?.Source.ToString();
    }

}