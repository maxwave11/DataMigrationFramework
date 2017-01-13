using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReplaceRule : ValueTransitionBase
    {
        [XmlAttribute]
        public string Rule { get; set; }

        public bool Important { get; set; }
        public string Condition { get; set; }
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

        public override TransitResult TransitValue(ValueTransitContext ctx)
        {
            TransitContinuation continuation = TransitContinuation.Continue;
            if (IfConditionIsTrue(ctx))
            {
                var result = base.TransitValue(ctx);
                continuation = Important ? TransitContinuation.SkipUnit : TransitContinuation.Continue;
            }

            return new TransitResult(continuation, ctx.TransitValue);
        }

        protected override object TransitValueInternal(ValueTransitContext ctx)
        {
            var value = ctx.TransitValue?.ToString();

            if (IfConditionIsTrue(ctx))
                value = GetReplacedValue(ctx);

            return value;
        }

        private string GetReplacedValue(ValueTransitContext ctx)
        {
            if (ReplaceExpression.Contains("{"))
            {
                return ExpressionEvaluator.EvaluateString(ReplaceExpression, ctx); 
            }
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
        private bool IfConditionIsTrue(ValueTransitContext ctx)
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

            if (Condition.StartsWith("{"))
            {
                return (bool)ExpressionEvaluator.Evaluate(Condition, ctx);
            }

            return transitValue?.Contains(Condition) ?? false;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "Rule: " + Rule;
        }
    }
}