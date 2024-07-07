using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataMigrationCore.Utils
{
    public static class StringUtils
    {
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNotEmpty(this string str)
        {
            return !IsEmpty(str);
        }

        public static string Join(this IEnumerable<string> collection, string sep = ",")
        {
            return string.Join(sep, collection);
        }

        public static string Truncate(this string value, int maxLength, string endsWith = "...")
        {
            if (value.IsEmpty()) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + endsWith;
        }

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new InfoJsonConverter() },
        };

        public static string GetInfoJson(this object dataObject)
        {
            try
            {
                return JsonSerializer.Serialize(dataObject, SerializerOptions);
            }
            catch (Exception e)
            {
                return "Error while serializing objects: \n" + e;
            }
        }
    }
}

