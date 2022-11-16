using System;

namespace DataMigration.Enums
{
    [Flags]
    public enum TraceMode
    {
        Auto     = 0, // 000000
        Objects  = 1, // 000001
        Commands = 3, // 000011
    }
}