using System;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{
    [Command("TYPE")]
    public class TypeConvertCommand : CommandBase
    {
        public string Format { get; set; }

        public string Type { get; set; }

        public char DecimalSeparator { get; set; }
        
        public CommandBase OnError { get; set; }


        private TypeCode _typeCode;

        public TypeConvertCommand()
        {
        }

        public TypeConvertCommand(string type)
        {
            Type = type;
        }

        private bool _isInitialized;
        
        public void Init()
        {
            if (_isInitialized)
                return;
            
            if (Type.ToLower() == "int")
                Type = "int32";

            if (Type.ToLower() == "float")
                Type = "single";

            if (Type.ToLower() == "bool")
                Type = "boolean";

            if (Type.ToLower() == "long")
                Type = "int64";
            
            if(!Enum.TryParse(Type, true, out _typeCode))
                throw new Exception($"Can't parse type name '{Type}'");
            
            _isInitialized = true;
        }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            Init();
            var value = ctx.TransitValue;
            char decimalSeparator = DecimalSeparator == 0 ? MapConfig.Current.DefaultDecimalSeparator : DecimalSeparator;
            
            try
            {
                var typedValue = TypeConverter.GetTypedValue(_typeCode, value, decimalSeparator, Format);
                ctx.SetCurrentValue(typedValue);
            }
            catch
            {
                if (OnError == null)
                    throw;

                ctx.Execute(OnError);
            }
        }


        public override string GetParametersInfo() => Type;
       
        public static implicit operator TypeConvertCommand(string type)
        {
            return new TypeConvertCommand(type);
        }
    }
}