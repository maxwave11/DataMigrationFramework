using System;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    
    public class ReadKeyTransition : ComplexTransition<ReadTransitUnit>
    {
        string _key = "";
        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            _key = "";
            Trace = MapConfig.Current.TraceKeyTransition;
            base.TransitInternal(ctx);
            return new TransitResult(_key.TrimEnd('/'));
        }
        protected override TransitResult TransitChild(ReadTransitUnit childTransition, ValueTransitContext ctx)
        {
            var result =  base.TransitChild(childTransition, ctx);
            _key += result.Value + "/";
            return new TransitResult(null);
        }

        public static implicit operator ReadKeyTransition(string expression)
        {
            var retVal =  new ReadKeyTransition() { expression };
           
            return retVal;
        }
    }   
    public class ReadTransitUnit : TransitUnit
    {
        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            var returnValue = Expression.IsJustString 
                ? ((IValuesObject)ctx.Source).GetValue(Expression.Expression)
                : Expression.Evaluate(ctx);

            return new TransitResult(returnValue);
        }
        
        public static implicit operator ReadTransitUnit(string expression)
        {
            return new ReadTransitUnit() { Expression = expression };
        }
    }   
    
   
}