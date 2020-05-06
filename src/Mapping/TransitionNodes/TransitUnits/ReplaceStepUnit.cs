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
        [XmlAttribute]
        public string Rule { get; set; }

        [XmlAttribute]
        public bool Important { get; set; }

        [XmlAttribute]
        public string Condition { get; set; }

        [XmlAttribute]
        public string ReplaceExpression { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (Rule.IsEmpty())
                return;

            if (!Rule.Contains('='))
                throw new Exception("Replace rule should contains condition and replace expression splitted by '='");

            Condition = Rule.Split('=')[0].Trim();
            if (Condition.StartsWith("!"))
            {
                Condition = Condition.TrimStart('!');
                Important = true;
            }
            ReplaceExpression = Rule.Split('=')[1];

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var continuation = TransitionFlow.Continue;
            var value = ctx.TransitValue?.ToString();

            if (ConditionIsTrue(ctx))
            {
                value = ReplaceValue(ctx);
                ctx.SetCurrentValue(this.Name, value);
                //continuation = Important ? TransitionFlow.SkipUnit : TransitionFlow.Continue;
            }

            return new TransitResult(continuation, value);
        }

        private string ReplaceValue(ValueTransitContext ctx)
        {
            // if (ReplaceExpression.Contains("{"))
            // {
            //     return ExpressionEvaluator.EvaluateString(ReplaceExpression, ctx); 
            // }
            if (Condition.StartsWith("@regexp:"))
            {
                var regex = new Regex(Condition.Replace("@regexp:", ""), RegexOptions.IgnoreCase);
                return regex.Replace(ctx.TransitValue.ToString(), ReplaceExpression);
            }

            if (Condition == "@empty" || Condition == "@any")
            {
                return ReplaceExpression;
            }

            return ctx.TransitValue?.ToString().Replace(Condition, ReplaceExpression);
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
            return base.ToString() + "Rule: " + Rule;
        }
    }
}