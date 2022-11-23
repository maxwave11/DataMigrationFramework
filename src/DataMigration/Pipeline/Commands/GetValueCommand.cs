using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataMigration.Enums;
using DataMigration.Pipeline.Expressions;
using DataMigration.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Puts the value from expression to pipeline
    /// </summary>
    [Yaml("GET")]
    public class GetValueCommand : CommandBase
    {
        private Func<ValueTransitContext, object> _func;
        public MigrationExpression Expression { get; init; }
        public Expression<Func<ValueTransitContext, object>> Expression2 { get; init; }
        
        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            object returnValue = null;
            if (Expression2 != null)
            {
                _func = Expression2.Compile();
                returnValue = _func(ctx);
            }
            else
            {
                returnValue = Expression.Evaluate(ctx);
            }

            ctx.SetCurrentValue(returnValue);
        }

        public override string GetParametersInfo()
        {
            return Expression2 != null ? Expression2.GetExpressionText() : Expression.ToString();
        }

        public static implicit operator GetValueCommand(string expression)
        {
            return new GetValueCommand() { Expression = expression };
        }
        
        public static implicit operator GetValueCommand(Expression<Func<ValueTransitContext, object>> expression)
        {
            return new GetValueCommand() { Expression2 = expression };
        }
        
        public string GetName(Expression e, out Expression parent)
        {   
            if(e is MemberExpression  m){ //property or field           
                parent = m.Expression;
                return m.Member.Name;
            }
            else if(e is MethodCallExpression mc){          
                string args = string.Join(",", mc.Arguments.SelectMany(GetExpressionParts));
                if(mc.Method.IsSpecialName){ //for indexers, not sure this is a safe check...           
                    return $"{GetName(mc.Object, out parent)}[{args}]";
                }
                else{ //other method calls      
                    parent = mc.Object;
                    return $"{mc.Method.Name}({args})";                     
                }
            }
            else if(e is ConstantExpression c){ //constant value
                parent = null;
                return c.Value?.ToString() ?? "null";       
            }
            else if(e is UnaryExpression u){ //convert
                parent=  u.Operand;
                return null;
            }
            else{
                parent =null;
                return e.ToString(); 
            }
        }

        public IEnumerable<string> GetExpressionParts(Expression e){
            var list = new List<string>();
            while(e!=null && !(e is ParameterExpression)){
                var name = GetName(e,out e);
                if(name!=null)list.Add(name);
            }
            list.Reverse();
            return list;
        }
        public string GetExpressionText<T>(Expression<Func<T, object>> expression) => string.Join(".", GetExpressionParts(expression.Body));
    }
    /// <summary>
    /// Puts not empty value from expression to pipeline. Do nothing if value empty
    /// </summary>
    [Yaml("GET?")]
    public sealed class GetNotEmptyValueCommand : GetValueCommand
    {
        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            base.ExecuteInternal(ctx);
            if (ExpressionContext.IsEmpty(ctx.TransitValue))
                ctx.Flow = TransitionFlow.SkipValue;
        }

        public static implicit operator GetNotEmptyValueCommand(string expression)
        {
            return new GetNotEmptyValueCommand() { Expression = expression };
        }
    }
}