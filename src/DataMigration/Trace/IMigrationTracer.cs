using System;
using DataMigration.Enums;
using DataMigration.Pipeline;

namespace DataMigration.Trace;

public interface IMigrationTracer
{
    void TraceMigrationException(string message, DataMigrationException ex);
    void TraceLine(string message, ValueTransitContext ctx = null, TraceMode level = TraceMode.Pipeline, ConsoleColor color = ConsoleColor.White);
    void Indent();
    void IndentBack();
}