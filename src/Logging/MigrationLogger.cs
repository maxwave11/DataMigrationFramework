using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataMigrationCore.Data;
using DataMigrationCore.Utils;
using Microsoft.Extensions.Logging;

namespace DataMigrationCore.Logging
{
    internal sealed class MigrationLogger : IMigrationLogger
    {
        private readonly ILogger _nativeLogger;
        private readonly LogLevel _overrideLogLevel;
        private int _identLevel;
        private readonly List<string> _lastTraceEntries = new();
        private object _migrationContext;
        private PipelineResults _pipelineResults = new();

        public MigrationLogger(ILogger nativeLogger, LogLevel overrideLogLevel)
        {
            _nativeLogger = nativeLogger;
            _overrideLogLevel = overrideLogLevel;
        }

        public void LogTrace(string message)
        {
            message = FormatMessage(message);
            _lastTraceEntries.Add((message));
            if (_overrideLogLevel == LogLevel.Trace && !_nativeLogger.IsEnabled(_overrideLogLevel))
            {
                _nativeLogger.LogInformation(message);
            }
            else
            {
                _nativeLogger.LogTrace(message);
            }
        }

        public void LogWarning(string message)
        {
            message = FormatMessage(message);
            _lastTraceEntries.Add((message));
            _nativeLogger.LogWarning(message);
        }

        public void LogInformation(string message)
        {
            message = FormatMessage(message);
            _lastTraceEntries.Add((message));
            _nativeLogger.LogInformation(message);
        }

        private void LogError(string message)
        {
            _nativeLogger.LogError(message);
        }

        public void SetContext(object context)
        {
            _migrationContext = context;
        }

        public void SetResults(PipelineResults results)
        {
            _pipelineResults = results;
        }

        public void TraceException(Exception ex, string correlationId)
        {
            var sb = new StringBuilder();

            sb.AppendLine("ERROR: " + ex.Message);

            sb.AppendLine("CorrelationId: " + correlationId);
            sb.AppendLine("TRACE:");

            Indent();
            sb.AppendJoin('\n', _lastTraceEntries);
            IndentBack();

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("CONTEXT:");
            Indent();
            sb.AppendLine(_migrationContext.GetInfoJson());
            IndentBack();

            sb.AppendLine();
            Indent();
            var objectsLog = LogObjects("RESULTS (Objects to Add):", _pipelineResults.ObjectsToAdd, 10);
            sb.AppendLine(objectsLog);
            IndentBack();

            sb.AppendLine();
            Indent();
            objectsLog = LogObjects("RESULTS (Objects to Update):", _pipelineResults.ObjectsToUpdate, 10);
            sb.AppendLine(objectsLog);
            IndentBack();

            _nativeLogger.LogError(ex, sb.ToString());
        }

        private string LogObjects(string message, IReadOnlyCollection<object> objects, int limit)
        {
            var sb = new StringBuilder(message);
            sb.AppendLine();
            sb.Append('{');
            foreach (var obj in objects.Take(limit))
            {
                sb.AppendLine();
                sb.Append($@"""{obj.GetType()}""");
                sb.Append(':');
                sb.Append(obj.GetInfoJson());
                sb.Append(',');
            }

            if (objects.Count > limit)
            {
                sb.AppendLine();
                sb.Append($"+ more {objects.Count - limit} objects ...");
            }

            sb.Append('}');

            return sb.ToString();
        }

        private string FormatMessage(string msg)
        {
            if (!msg.Contains('\n'))
                return GetIdentString() + msg;

            return msg.Split('\n').Select(i => GetIdentString() + i).Join("\n");
        }

        private string GetIdentString()
        {
            return new string(' ', _identLevel * 2);
        }

        public void Indent()
        {
            _identLevel++;
        }

        public void IndentBack()
        {
            _identLevel--;
        }

        public void ClearLastLogs()
        {
            _lastTraceEntries.Clear();
        }

        public IDisposable BeginScope(string message)
        {
            return _nativeLogger.BeginScope(message);
        }
    }
}
