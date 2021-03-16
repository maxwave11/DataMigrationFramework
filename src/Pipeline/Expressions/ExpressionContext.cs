using System;
using System.Collections.Generic;
using System.Globalization;
using DataMigration.Data;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Expressions
{
    /// <summary>
    /// Object of this class represents the contex for accessing from migration expression in run-time
    /// You can use any public member/method of this class from expression.
    /// Examples: { HISTORY('fieldName') } - returns result of ExpressionContext.HISTORY method
    /// Examples: { SRC['fieldName'] } - returns value of 'fieldName' field of source object in current object transition  (ExpressionContext.SRC property)
    /// </summary>
    public class ExpressionContext
    {
        public IDataObject SRC => _ctx.Source;
        public IDataObject TARGET => _ctx.Target;
        
        /// <summary>
        /// Current transit value. Use this property in expressions to handle value on transit pipeline
        /// </summary>
        public object VALUE => _ctx.TransitValue;
        
        public IDataObject DataObject => (IDataObject)_ctx.TransitValue;
        
        /// <summary>
        /// Global configuration variables accessor
        /// </summary>
        public Dictionary<string, object> Variables => MapConfig.Current.Variables;

        /// <summary>
        /// Global configuration mappings accessor
        /// </summary>
        public Dictionary<string, string> Mappings => MapConfig.Current.Mappings;


        private readonly ValueTransitContext _ctx;

        public ExpressionContext(ValueTransitContext ctx)
        {
            _ctx = ctx;
        }

        public string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }
        public string Str(object value)
        {
            return value?.ToString() ?? "";
        }

        public string Format(string format, object argument)
        {
            if (argument?.ToString().IsEmpty() == true)
                return string.Empty;

            return String.Format(format,argument);
        }
        
        public T GetValueFromSource<T>(string name)
        {
            return TypeConverter.GetTypedValue<T>(SRC[name], MapConfig.Current.DefaultDecimalSeparator) ;
        }


        public object Field(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            if (obj is DataObject)
                return ((DataObject)obj)[fieldName];

            return FastReflection.GetValue(obj, fieldName);
        }
     

        public static bool IsEmpty(object obj)
        {
            switch (obj)
            {
                case null:
                    return true;
                case string s:
                    return s.IsEmpty();
                default:
                    return false;
            }
        }

        public string CapitalizeWordLetters(object str)
        {
            if (str == null)
                return null;

            var myTi = new CultureInfo("en-US", false).TextInfo;
            return myTi.ToTitleCase(str.ToString().ToLower());
        }
    }
}