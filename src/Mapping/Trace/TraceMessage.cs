using System;

namespace XQ.DataMigration.Mapping.Trace
{
    public class TraceMessage
    {
        public string Text { get; private set; }
        public ConsoleColor Color { get; private set; }

        public TraceMessage(string text, ConsoleColor color)
        {
            Text = text;
            Color = color;
        }
    }
}