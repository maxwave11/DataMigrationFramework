using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataMigrationCore.Utils;

public class InfoJsonConverter : JsonConverter<object>
{
    private const int MaximumCollectionOutput = 10;

    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static readonly JsonSerializerOptions InnerSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        IgnoreReadOnlyFields = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    private static readonly JsonSerializerOptions ArraySerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
    };

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        WriteInternal(writer, value, options, 0);
    }

    private void WriteInternal(Utf8JsonWriter writer, object value, JsonSerializerOptions options, int nestingLevel)
    {
        if (value is null || value.GetType().IsValueType || value is string)
        {
            JsonSerializer.Serialize(writer, value, InnerSerializerOptions);
            return;
        }

        if (value is ICollection collection)
        {
            WriteCollection(writer, collection, options, nestingLevel);
            return;
        }

        WriteObject(writer, value, options, nestingLevel);
    }


    private void WriteObject(Utf8JsonWriter writer, object value, JsonSerializerOptions options, int nestingLevel)
    {
        writer.WriteStartObject();
        writer.WriteString("__type", value.ToString());
        WriteObjectProperties(writer, value, options, nestingLevel);
        writer.WriteEndObject();
    }

    private void WriteObjectProperties(Utf8JsonWriter writer, object value, JsonSerializerOptions options, int nestingLevel)
    {
        if (nestingLevel > 3)
        {
            writer.WriteStringValue(" { ... } ");
            return;
        }

        foreach (var property in value.GetType().GetProperties())
        {
            if (property.GetIndexParameters().Length > 0)
                continue;

            writer.WritePropertyName(property.Name);
            WriteInternal(writer, property.GetValue(value), options, nestingLevel + 1);
        }
    }


    private void WriteCollection(Utf8JsonWriter writer, ICollection collection, JsonSerializerOptions options, int nestingLevel)
    {
        if (nestingLevel > 3)
        {
            writer.WriteStringValue($"({collection.Count})[ ... ]");
            return;
        }
        writer.WriteStartArray();

        int counter = 0;
        foreach (var dataObject in collection)
        {
            if (counter > MaximumCollectionOutput)
                break;

            WriteInternal(writer, dataObject, options, nestingLevel + 1);
            counter++;
        }

        if (counter < collection.Count)
        {
            WriteInternal(writer, $"... more {collection.Count - counter} items", options, nestingLevel + 1);
        }

        writer.WriteEndArray();
    }
}
