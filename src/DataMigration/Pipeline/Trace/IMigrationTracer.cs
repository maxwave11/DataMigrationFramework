using System;

namespace DataMigration.Pipeline.Trace;

public interface IMigrationTracer
{
    void TraceLine(string message, ValueTransitContext ctx = null, ConsoleColor color = ConsoleColor.White);
}