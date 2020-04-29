using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Scripting;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions
{

    public class FromFieldMigrationExpression: MigrationExpression
    {
        public FromFieldMigrationExpression(string expression) : base(expression)
        {
        
        }
        
        public static implicit operator FromFieldMigrationExpression(string expression)
        {
            var expr = expression.Contains("{") ? expression : $"{{ SRC[{expression}] }}";
              
            return new FromFieldMigrationExpression(expr);
        }
    }

    public class MigrationExpression
    {
        protected string _expression;
        
        private readonly ScriptRunner<object> _scriptRunner;
        Expressions.ExpressionEvaluator ExpressionEvaluator { get; } = new Expressions.ExpressionEvaluator();

        
        public MigrationExpression(string expression)
        {

            if (expression.StartsWith("#"))
            {
                _expression = $"{{ VALUE[{ expression }] }}";
            }
            else if (expression.StartsWith("$"))
            {
                _expression = expression;
            }
            else
                throw new InvalidOperationException("Invalid expression");
            
            _scriptRunner = ExpressionCompiler.Compile(_expression, null);
        }

        public object Evaluate(ValueTransitContext ctx)
        {
            return ExpressionEvaluator.Evaluate(_expression, ctx);
        }
        
        public string EvaluateString(string expression, ValueTransitContext ctx)
        {
            if (expression.IsEmpty())
                return expression;

            return Evaluate(expression, ctx)?.ToString();
        }

        public object Evaluate(string expression, ValueTransitContext ctx)
        {
            //don't evaluate passed plain strings
            if (!expression.Contains("{"))
                return expression;

            try
            {
                var exprContext = new ExpressionContext(ctx);
                var task = _scriptRunner(exprContext);
                if (!task.IsCompleted)
                    throw new Exception("TASK NOT COMPLETED!!! ALARM!");
                
                var value = task.Result;
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public static implicit operator MigrationExpression(string expression)
        {
            return new MigrationExpression(expression);
        }
    }

    public class TransitValueCommand : ComplexTransition
    {
        public FromFieldMigrationExpression From { get; set; }

        [XmlAttribute]
        public string Replace { get; set; }

        [XmlAttribute]
        public string DataType { get; set; }

        [XmlAttribute]
        public string DecimalSeparator { get; set; } = ".";

        [XmlAttribute]
        public string DataTypeFormat { get; set; }

        [XmlAttribute]
        public string Format { get; set; }

        [XmlAttribute]
        public MigrationExpression To { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (Pipeline == null)
                Pipeline = new List<TransitionNode>();

            Color = ConsoleColor.Green;
            
            
            InitializeChildTransitions();

            base.Initialize(parent);
        }

        protected virtual void InitializeChildTransitions()
        {
            var userDefinedTransitions = Pipeline.ToList();
            Pipeline?.Clear();

            InsertReadTransitUnit();
            InsertCustomTransitions(userDefinedTransitions);
            InsertReplaceTransitUnit();
            InsertDataTypeConvertTransitUnit();
            InsertWriteTransitUnit();
        }

        protected virtual void InsertCustomTransitions(List<TransitionNode> userDefinedTransitions)
        {
            Pipeline.AddRange(userDefinedTransitions);
        }

        protected virtual void InsertReadTransitUnit()
        {
            Pipeline.Add(new TransitUnit() {Expression = From, OnError = this.OnError});
        }

        protected virtual void InsertReplaceTransitUnit()
        {
            if (Replace.IsNotEmpty())
            {
                Pipeline.Add(new ReplaceTransitUnit
                {
                    ReplaceExpression = Replace,
                    OnError = this.OnError
                });
            }
        }

        protected virtual void InsertDataTypeConvertTransitUnit()
        {
            if (DataType.IsNotEmpty())
            {
                Pipeline.Add(new TypeConvertTransitUnit
                {
                    DataType = DataType,
                    DataTypeFormats = DataTypeFormat,
                    DecimalSeparator = DecimalSeparator,
                    OnError = this.OnError
                });
            }
        }

        //WriteTransitUnit must be always last transtion in ChildTransitions collection
        protected virtual void InsertWriteTransitUnit()
        {
            if (To != null)
            {
                Pipeline.Add(new WriteTransitUnit()
                {
                    Expression = To,
                    OnError = this.OnError
                });
            }
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes = $"From=\"{From}\" To=\"{To}\"";
            base.TraceStart(ctx, attributes);
        }


        public static implicit operator TransitValueCommand(string expression)
        {
            return new TransitValueCommand() { From = expression };
        }
    }
}