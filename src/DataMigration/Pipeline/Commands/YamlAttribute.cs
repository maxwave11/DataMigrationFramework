using System;

namespace DataMigration.Pipeline.Commands;

/// <summary>
/// Assign TAG name to the command. You can use it in YAML configuration
/// instead of full command class name
/// </summary>
public class YamlAttribute: Attribute
{
    public string TokenName { get; }
    public YamlAttribute(string tokenName)
    {
        TokenName = tokenName;
    }
}