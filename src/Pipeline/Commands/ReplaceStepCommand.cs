using System;
using System.Text.RegularExpressions;
using DataMigration.Enums;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{
    [Yaml("REPLACE_STEP")]
    public class ReplaceStepCommand : CommandBase
    {
        public bool Important { get; set; }

        public string Condition { get; set; }

        public string ReplaceValue { get; set; }

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            if (ConditionIsTrue(ctx))
            {
                var value = Replace(ctx);
                ctx.SetCurrentValue(value);
                ctx.Flow = Important ? TransitionFlow.SkipValue : TransitionFlow.Continue;
            }
        }

        private string Replace(ValueTransitContext ctx)
        {
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

            return transitValue?.Contains(Condition) ?? false;
        }

        public override string GetParametersInfo() => $"Condition: {Condition}, ReplaceValue: {ReplaceValue}";
        
        public static implicit operator ReplaceStepCommand(string replaceExpression)
        {
            if (replaceExpression.IsEmpty())
                throw new Exception("Replace expression cant be empty");

            if (!replaceExpression.Contains('='))
                throw new Exception("Replace rule should contains condition and replace value splitted by '='");

            return new ReplaceStepCommand()
            {
                Important = replaceExpression.StartsWith("!"),
                Condition = replaceExpression.Split('=')[0].Trim().TrimStart('!'),
                ReplaceValue = replaceExpression.Split('=')[1],
            };
        }
    }
}