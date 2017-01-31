using System;
using System.Collections.Generic;
using XQ.DataMigration.Mapping;

namespace XQ.DataMigration
{
    internal static class TransitLogger
    {
        public static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            Migrator.Current.InvokeLog(message);
            //NLog.GlobalDiagnosticsContext.Set("color", color);
            //NLog.LogManager.GetLogger("InfoLogger").Info(message);
        }
    }
}
