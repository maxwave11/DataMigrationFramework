using System;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    [Command("TYPE")]
    public class TypeConvertCommand : CommandBase
    {
        public string DataTypeFormats { get; set; }

        public string DataType { get; set; }

        public char DecimalSeparator { get; set; }

        private readonly TypeCode _typeCode;

        public TypeConvertCommand()
        {
        }

        public TypeConvertCommand(string type)
        {
            DataType = type;
            if (type.ToLower() == "int")
                DataType = "int32";

            if (type.ToLower() == "float")
                DataType = "single";

            if (type.ToLower() == "bool")
                DataType = "boolean";

            if (type.ToLower() == "long")
                DataType = "int64";
            
            if(!Enum.TryParse(DataType, true, out _typeCode))
                throw new Exception($"Can't parse type name '{DataType}'");
        }
        protected override void ExecuteInternal(ValueTransitContext ctx)
        {
            var value = ctx.TransitValue;
            var formats = DataTypeFormats.IsNotEmpty() ? DataTypeFormats.Split(',') : null;
            var decimalSeparator = DecimalSeparator == 0 ? MapConfig.Current.DefaultDecimalSeparator: DecimalSeparator;
            var typedValue = TypeConverter.GetTypedValue(_typeCode, value, decimalSeparator, formats);
            ctx.SetCurrentValue(typedValue);
        }

        public override string ToString()
        {
            return DataType;
        }

        public static implicit operator TypeConvertCommand(string type)
        {
            return new TypeConvertCommand(type);
        }
    }
}