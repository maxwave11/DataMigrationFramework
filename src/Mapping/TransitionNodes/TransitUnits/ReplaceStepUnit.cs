using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReplaceStepUnit : TransitUnit
    {
        public bool Important { get; set; }

        public string Condition { get; set; }

        public string ReplaceValue { get; set; }

        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            var continuation = TransitionFlow.Continue;
            var value = ctx.TransitValue?.ToString();

            if (ConditionIsTrue(ctx))
            {
                value = Replace(ctx);
                ctx.SetCurrentValue(this.Name, value);
                //continuation = Important ? TransitionFlow.SkipUnit : TransitionFlow.Continue;
            }

            return new TransitResult(continuation, value);
        }

        private string Replace(ValueTransitContext ctx)
        {
            // if (ReplaceValue.Contains("{"))
            // {
            //     return ExpressionEvaluator.EvaluateString(ReplaceValue, ctx); 
            // }
            if (Condition.StartsWith("@regexp:"))
            {
                var regex = new Regex(Condition.Replace("@regexp:", ""), RegexOptions.IgnoreCase);
                return regex.Replace(ctx.TransitValue.ToString(), ReplaceValue);
            }

            if (Condition == "@empty" || Condition == "@any")
            {
                return ReplaceValue;
            }

            return ctx.TransitValue?.ToString().Replace(Condition, ReplaceValue);
        }
        private bool ConditionIsTrue(ValueTransitContext ctx)
        {
            string transitValue = ctx.TransitValue?.ToString();

            switch (Condition)
            {
                case "@any":
                    return true;
                case "@empty":
                    return string.IsNullOrEmpty(transitValue);
            }

            if (Condition.StartsWith("@regexp:"))
            {
                var regex = new Regex(Condition.Replace("@regexp:", ""), RegexOptions.IgnoreCase);
                return regex.IsMatch(transitValue);
            }

            //if (Condition.StartsWith("{"))
            //{
            //    return (bool)ExpressionEvaluator.Evaluate(Condition, ctx);
            //}

            return transitValue?.Contains(Condition) ?? false;
        }

        public override string ToString()
        {
            return base.ToString() + $"Condition: { Condition }, ReplaceValue: { ReplaceValue }";
        }
        
        public static implicit operator ReplaceStepUnit(string replaceExpression)
        {
            if (replaceExpression.IsEmpty())
                throw new Exception("Replace expression cant be empty");

            if (!replaceExpression.Contains('='))
                throw new Exception("Replace rule should contains condition and replace value splitted by '='");

            return new ReplaceStepUnit()
            {
                Important = replaceExpression.StartsWith("!"),
                Condition = replaceExpression.Split('=')[0].Trim().TrimStart('!'),
                ReplaceValue = replaceExpression.Split('=')[1],
            };
        }
    }
}