using System;
using System.Collections.Generic;
using NLog;

namespace XQ.DataMigration
{
    public class TransitLogger
    {
        public static bool IsConsoleSaveLogsEnabled { get; set; }
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string info, ConsoleColor color = ConsoleColor.White)
        {
            NLog.GlobalDiagnosticsContext.Set("color", color);
            NLog.LogManager.GetLogger("InfoLogger").Info(info);
        }

        private static List<string> headers = new List<string>();
    }
}
