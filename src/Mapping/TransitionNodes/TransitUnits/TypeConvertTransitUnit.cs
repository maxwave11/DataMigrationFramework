using System;
using System.Globalization;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class TypeConvertTransitUnit : TransitUnit
    {
        [XmlAttribute]
        public string DataTypeFormat { get; set; }

        [XmlAttribute]
        public string DataType { get; set; }

        private TypeCode _typeCode;
        public override void Initialize(TransitionNode parent)
        {
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
        
        protected override object TransitValueInternal(ValueTransitContext ctx)
        {
            var value = base.TransitValueInternal(ctx);
            return  GetTypedValue(_typeCode, value, DataTypeFormat);
        }

        public static  object GetTypedValue(TypeCode targetType, object value, string DataTypeFormat = null)
        {
            if (value == null || value.ToString().IsEmpty()) return null;

            switch (targetType)
            {
                case TypeCode.Int32:
                    if (value.ToString().IndexOfAny(new[] { ',', '.' }) > 0)
                        return Convert.ToInt32(GetTypedValue(TypeCode.Double, value, DataTypeFormat), CultureInfo.InvariantCulture);

                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);

                case TypeCode.Int64:
                    if (value.ToString().IndexOfAny(new[] { ',', '.' }) > 0)
                        return Convert.ToInt64(GetTypedValue(TypeCode.Double, value, DataTypeFormat), CultureInfo.InvariantCulture);

                    return Convert.ToInt64(value, CultureInfo.InvariantCulture);

                case TypeCode.Single:
                    return Convert.ToSingle(value.ToString().Replace(',', '.').Replace(" ", String.Empty), CultureInfo.InvariantCulture);

                case TypeCode.Double:
                    try
                    {
                        return Convert.ToDouble(value.ToString().Replace(',', '.').Replace(" ", String.Empty), CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        //возможно это doble, преобразованный в дату SourceProvider'ом
                        var date = Convert.ToDateTime(value).ToString("dd.M");
                        return Convert.ToDouble(date, CultureInfo.InvariantCulture);
                        throw;
                    }
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value.ToString().Replace(',', '.').Replace(" ", String.Empty), CultureInfo.InvariantCulture);

                case TypeCode.String:
                    return Convert.ToString(value);

                case TypeCode.Boolean:
                    return ToBool(value);
                case TypeCode.DateTime:
                    if (value is string && DataTypeFormat.IsNotEmpty())
                    {
                        return DateTime.ParseExact(value.ToString(), DataTypeFormat, new DateTimeFormatInfo());
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
            catch (Exception ex)
            {
                return Convert.ToBoolean(Convert.ToInt32(value));
            }
        }
    }
}