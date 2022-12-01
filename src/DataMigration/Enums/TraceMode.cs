using System;

namespace DataMigration.Enums
{
    [Flags]
    public enum TraceMode
    {
        Pipeline = 0, // 00000
        Object = 1, // 00001
        Pipes = 3, // 00011
    }
}