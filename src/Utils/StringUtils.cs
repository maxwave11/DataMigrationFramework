using System;
using System.Collections.Generic;

namespace DataMigration.Utils
{
    public static class StringUtils
    {
        public static bool IsEmpty(this string str)
        {
            return String.IsNullOrEmpty(str?.Trim());
        }

        public static bool IsNotEmpty(this string str)
        {
            return !IsEmpty(str);
        }

		public static string Join(this IEnumerable<string> collection, string sep=",")
		{
			return String.Join(sep, collection);
		}

        public static string Truncate(this string value, int maxLength, string endsWith = "...")
        {
            if (value.IsEmpty()) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + endsWith;
        }
    }
}
