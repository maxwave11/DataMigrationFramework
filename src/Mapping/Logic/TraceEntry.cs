using System;

namespace XQ.DataMigration.Mapping.Logic
{
    public class TraceEntry
    {
        public string Mesage { get; set; }
        public ConsoleColor Color { get; set; }

        public override string ToString()
        {
            return Mesage;
        }
    }
}