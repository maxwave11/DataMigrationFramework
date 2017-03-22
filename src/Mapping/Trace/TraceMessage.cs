using System;
using XQ.DataMigration.Mapping.TransitionNodes;

namespace XQ.DataMigration.Mapping.Trace
{
    public class TraceMessage
    {
        public string Text { get; private set; }
        public ConsoleColor Color { get; private set; }
        public TransitionNode TransitionNode { get; }
        public TraceMessage(string text, ConsoleColor color, TransitionNode node)
        {
            Text = text;
            Color = color;
            TransitionNode = node;
        }
    }
}