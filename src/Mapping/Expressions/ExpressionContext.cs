using System;
using System.Dynamic;
using System.Globalization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.Expressions
{
    /// <summary>
    /// Object of this class represents the contex for accessing from migration expression in run-time
    /// You can use any public member/method of this class from expression.
    /// Examples: { HISTORY('fieldName') } - returns result of ExpressionContext.HISTORY method
    /// Examples: { SRC['fieldName'] } - returns value of 'fieldName' field of source object in current object transition  (ExpressionContext.SRC property)
    /// </summary>
    public class ExpressionContext
    {
        public IValuesObject SRC => _ctx.Source;
        public IValuesObject TARGET => _ctx.Target;
        public object VALUE => _ctx.TransitValue;
        public IValuesObject GLOBAL => GlobalObjectTransition.GlobalObject;
        //object for temporary storing any IValuesObject during migration whic avalilable during whole migration process
        public IValuesObject CUSTOM { get => GlobalObjectTransition.CustomObject; set => GlobalObjectTransition.CustomObject = value; }
        public ObjectTransition THIS => _ctx.ObjectTransition;        
        private readonly ValueTransitContext _ctx;

        public ExpressionContext(ValueTransitContext ctx)
        {
            _ctx = ctx;
        }

        public object HISTORY(string transitionName)
        {
            return _ctx.GetHistoricValue(transitionName);
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

        public object Field(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            if (obj is ValuesObject)
                return ((ValuesObject)obj)[fieldName];

            return FastReflection.GetValue(obj, fieldName);
        }
        public bool IsNotEmpty(string str)
        {
            return str.IsNotEmpty();
        }

        public bool IsNullOrEmptyString(object obj)
        {
            if (obj == null)
                return true;

            if (obj is string)
            {
                return ((string)obj).IsEmpty();
            }
            return false;
        }

        public string CapitalizeWordLetters(object str)
        {
            if (str == null)
                return null;

            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            return myTI.ToTitleCase(str.ToString().ToLower());
        }
    }
}