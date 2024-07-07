using System;
using DataMigrationCore.Data;

namespace DataMigrationCore.Logging;

public interface IMigrationLogger
{
    void SetContext(object migrationContext);
    void SetResults(PipelineResults results);
    void TraceException(Exception ex, string correlationId);
    void Indent();
    void IndentBack();
    void ClearLastLogs();
    void LogTrace(string message);
    void LogWarning(string message);
    void LogInformation(string message);
    IDisposable BeginScope(string message);
}
