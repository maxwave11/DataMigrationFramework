using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace XQ.DataMigration.Utils
{
    public static class TypeConverter
    {
        public static T GetTypedValue<T>(object value, char decimalSeparator = '.', string dataFormat = null)
        {
            if (default(T) == null && value == null)
                return default(T);

            var typeCode = GetTypeCode(typeof(T));

            var convertedValue = GetTypedValue(typeCode, value,decimalSeparator, dataFormat);

            return (T)convertedValue;
        }

        private static TypeCode GetTypeCode(Type type)
        {
            if (type == typeof(int) || type == typeof(int?))
                return TypeCode.Int32;
            if (type == typeof(long) || type == typeof(long?))
                return TypeCode.Int64;
            if (type == typeof(double) || type == typeof(double?))
                return TypeCode.Double;
            if (type == typeof(float) || type == typeof(float?))
                return TypeCode.Single;
            if (type == typeof(decimal) || type == typeof(decimal?))
                return TypeCode.Decimal;
            if (type == typeof(bool) || type == typeof(bool?))
                return TypeCode.Boolean;
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return TypeCode.DateTime;
            if (type == typeof(string))
                return TypeCode.String;

            throw new NotSupportedException($"Type {type} is not supported for conversion");
        }


        public static object GetTypedValue(TypeCode targetType, object value, char decimalSeparator, string dataFormat = null)
        {
            if (value == null)
                return null;

            var strValue = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (strValue.IsEmpty())
                return null;

            switch (targetType)
            {
                case TypeCode.Int32:
                    if (strValue.Contains(decimalSeparator))
                        return Convert.ToInt32(GetTypedValue(TypeCode.Double, value, decimalSeparator), CultureInfo.InvariantCulture);

                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);

                case TypeCode.Int64:
                    if (strValue.Contains(decimalSeparator))
                        return Convert.ToInt64(GetTypedValue(TypeCode.Double, value, decimalSeparator), CultureInfo.InvariantCulture);

                    return Convert.ToInt64(strValue.Replace(" ",""), CultureInfo.InvariantCulture);

                case TypeCode.Single:
                    return Convert.ToSingle(PrepareDecimalValue(value,decimalSeparator), CultureInfo.InvariantCulture);

                case TypeCode.Double:
                    try
                    {
                        return Convert.ToDouble(PrepareDecimalValue(value,decimalSeparator), CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        //возможно это double, преобразованный в дату SourceProvider'ом
                        var date = Convert.ToDateTime(value).ToString("dd.M");
                        return Convert.ToDouble(date, CultureInfo.InvariantCulture);
                        throw;
                    }

                case TypeCode.Decimal:
                    return Convert.ToDecimal(PrepareDecimalValue(value,decimalSeparator), CultureInfo.InvariantCulture);

                case TypeCode.String:
                    return Convert.ToString(value);

                case TypeCode.Boolean:
                    return ToBool(value);

                case TypeCode.DateTime:
                    if (value is string)
                    {
                        return DateTime.ParseExact(strValue.Trim(), dataFormat.IsEmpty() ? "d.M.yyyy" : dataFormat, CultureInfo.InvariantCulture);
                    }
                    return Convert.ToDateTime(value);
            }
                   
            throw new NotSupportedException($"Type {targetType} is not supported");
        }

        private static bool ToBool(object value)
        {
            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return Convert.ToBoolean(Convert.ToInt32(value));
            }
        }

        private static string PrepareDecimalValue(object value, char decimalSeparator)
        {
            
            var strValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (decimalSeparator == '.')
                strValue = strValue.Replace(",", "").Replace(" ","");
            else
            {
                strValue = strValue.Replace(decimalSeparator.ToString(), ".");
            }
            
            //return string without any white spaces (don't use String.Replace for that)
            return Regex.Replace(strValue, @"\s+", "");
        }
    }
}