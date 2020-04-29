using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions
{
    public class TransitValueCommand : ComplexTransition
    {
        [XmlAttribute]
        public string From { get; set; }

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
        public string To { get; set; }

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
            if (From.IsNotEmpty())
            {
                Pipeline.Add(new ReadTransitUnit() {From = From, OnError = this.OnError});
            }
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
            if (To.IsNotEmpty())
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