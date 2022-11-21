using System;
using DataMigration.Pipeline.Commands;

namespace DataMigration.Utils
{
    public static class CommandUtils{
        public static string GetCommandYamlName(Type commandType)
        {
            var attribute = (YamlAttribute)Attribute.GetCustomAttribute(commandType, typeof (YamlAttribute));
            
            return attribute?.TokenName ?? commandType.Name;
        }
    }
}