using System;

namespace DataMigration;

public interface IMigrationLogger
{
    void Log(ConsoleColor color, string text);
}