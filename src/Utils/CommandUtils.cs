using System;
using DataMigration.Pipeline.Commands;

namespace DataMigration.Utils
{
    public static class CommandUtils{
        public static string GetCommandYamlName(Type commandType)
        {
            var attribute = (CommandAttribute)Attribute.GetCustomAttribute(commandType, typeof (CommandAttribute));
            
            return attribute?.Name ?? commandType.Name;
        }
    }
}