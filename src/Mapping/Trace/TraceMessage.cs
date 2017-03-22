using System;

namespace XQ.DataMigration.Mapping.Trace
{
    public class TraceMessage
    {
        public string Text { get; private set; }
        public ConsoleColor Color { get; private set; }
        public bool IsUserMessage { get; private set; }

        public TraceMessage(string text, ConsoleColor color, bool isUserMessage = false)
        {
            Text = text;
            Color = color;
            IsUserMessage = isUserMessage;
        }
    }
}