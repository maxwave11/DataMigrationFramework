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

        protected override void TransitInternal(ValueTransitContext ctx)
        {
            var value = ctx.TransitValue;
            var formats = DataTypeFormats.IsNotEmpty() ? DataTypeFormats.Split(',') : null;
            
            var typedValue = TypeConverter.GetTypedValue(_typeCode, value, DecimalSeparator, formats);
            ctx.SetCurrentValue(typedValue);
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