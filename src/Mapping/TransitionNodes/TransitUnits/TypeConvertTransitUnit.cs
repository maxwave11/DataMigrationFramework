using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TypeConvertTransitUnit : TransitionNode
    {
        public string DataTypeFormats { get; set; }

        public string DataType { get; set; }

        public char DecimalSeparator { get; set; }

        private TypeCode _typeCode;
        public override void Initialize(TransitionNode parent)
        {
            if (DecimalSeparator == 0)
                DecimalSeparator = MapConfig.Current.DefaultDecimalSeparator;
                
            if (DataType.ToLower() == "int")
                DataType = "int32";

            if (DataType.ToLower() == "float")
                DataType = "single";

            if (DataType.ToLower() == "bool")
                DataType = "boolean";

            if (DataType.ToLower() == "long")
                DataType = "int64";

            if(!Enum.TryParse(DataType, true, out _typeCode))
                throw new Exception($"Can't parse type name '{DataType}'");
            
            base.Initialize(parent);
        }

        protected override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            var value = ctx.TransitValue;
            var typedValue = GetTypedValue(_typeCode, value, DataTypeFormats.IsNotEmpty() ? DataTypeFormats.Split(',') : null);
            return new TransitResult(typedValue);
        }

        public object GetTypedValue(TypeCode targetType, object value, string[] DataTypeFormats = null)
        {
            if (value == null)
                return null;

            var strValue = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (strValue.IsEmpty())
                return null;

            switch (targetType)
            {
                case TypeCode.Int32:
                    if (strValue.Contains(DecimalSeparator))
                        return Convert.ToInt32(GetTypedValue(TypeCode.Double, value, DataTypeFormats), CultureInfo.InvariantCulture);

                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);

                case TypeCode.Int64:
                    if (strValue.Contains(DecimalSeparator))
                        return Convert.ToInt64(GetTypedValue(TypeCode.Double, value, DataTypeFormats), CultureInfo.InvariantCulture);

                    return Convert.ToInt64(strValue.Replace(" ",""), CultureInfo.InvariantCulture);

                case TypeCode.Single:
                    return Convert.ToSingle(PrepareDecimalValue(value), CultureInfo.InvariantCulture);

                case TypeCode.Double:
                    try
                    {
                        return Convert.ToDouble(PrepareDecimalValue(value), CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        //возможно это double, преобразованный в дату SourceProvider'ом
                        var date = Convert.ToDateTime(value).ToString("dd.M");
                        return Convert.ToDouble(date, CultureInfo.InvariantCulture);
                        throw;
                    }

                case TypeCode.Decimal:
                    return Convert.ToDecimal(PrepareDecimalValue(value), CultureInfo.InvariantCulture);

                case TypeCode.String:
                    return Convert.ToString(value);

                case TypeCode.Boolean:
                    return ToBool(value);

                case TypeCode.DateTime:
                    if (value is string)
                    {
                        //set default date formats to parse
                        DataTypeFormats = DataTypeFormats?.Any() == true ? DataTypeFormats : new[] {"dd.MM.yyyy","d.M.yyyy","dd.M.yyyy","d.MM.yyyy"};
                        return DateTime.ParseExact(strValue.Trim(), DataTypeFormats, new DateTimeFormatInfo(), DateTimeStyles.None);
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

        private string PrepareDecimalValue(object value)
        {
            
            var strValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (DecimalSeparator == '.')
                strValue = strValue.Replace(",", "").Replace(" ","");
            else
            {
                strValue = strValue.Replace(DecimalSeparator.ToString(), ".");
            }
            
            //return string without any white spaces (don't use String.Replace for that)
            return Regex.Replace(strValue, @"\s+", "");
        }

        public override string ToString()
        {
            return $"TargetType=\"{ DataType }\"";
        }

        public static implicit operator TypeConvertTransitUnit(string expression)
        {
            return new TypeConvertTransitUnit() { DataType = expression };
        }
    }
}