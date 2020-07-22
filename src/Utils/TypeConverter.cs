using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using XQ.DataMigration.Data;

namespace XQ.DataMigration.Utils
{
    public static class TypeConverter
    {
        public static T GetTypedValue<T>(object value, char decimalSeparator = '.', string dataFormat = null)
        {

            TypeCode typeCode = TypeCode.Object;

            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                typeCode = TypeCode.Int32;
            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                typeCode = TypeCode.Int64;
            if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                typeCode = TypeCode.Double;
            if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
                typeCode = TypeCode.Single;
            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                typeCode = TypeCode.Decimal;
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
                typeCode = TypeCode.Boolean;
            if (typeof(T) == typeof(string) )
                typeCode = TypeCode.String;
            if (typeof(T) == typeof(DateTime)|| typeof(T) == typeof(DateTime?) )
                typeCode = TypeCode.DateTime;


            if (default(T) == null && value == null)
                return default(T);

            var converterdValue = GetTypedValue(typeCode, value,decimalSeparator, dataFormat);

            return (T)converterdValue;
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
                   
            throw new NotSupportedException($"Type {targetType} are not supported");
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