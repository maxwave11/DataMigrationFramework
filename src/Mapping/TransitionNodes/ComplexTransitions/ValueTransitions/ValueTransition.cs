using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions
{
    public class ValueTransition : ComplexTransition
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
            if (ChildTransitions == null)
                ChildTransitions = new List<TransitionNode>();

            Color = ConsoleColor.Green;
            
            InitializeChildTransitions();

            base.Initialize(parent);
        }

        protected virtual void InitializeChildTransitions()
        {
            var userDefinedTransitions = ChildTransitions.ToList();
            ChildTransitions?.Clear();

            InsertReadTransitUnit();
            InsertCustomTransitions(userDefinedTransitions);
            InsertReplaceTransitUnit();
            InsertDataTypeConvertTransitUnit();
            InsertWriteTransitUnit();
        }

        protected virtual void InsertCustomTransitions(List<TransitionNode> userDefinedTransitions)
        {
            ChildTransitions.AddRange(userDefinedTransitions);
        }

        protected virtual void InsertReadTransitUnit()
        {
            if (From.IsNotEmpty())
            {
                ChildTransitions.Add(new ReadTransitUnit() {From = From, OnError = this.OnError});
            }
        }

        protected virtual void InsertReplaceTransitUnit()
        {
            if (Replace.IsNotEmpty())
            {
                ChildTransitions.Add(new ReplaceTransitUnit
                {
                    ReplaceRules = Replace,
                    OnError = this.OnError
                });
            }
        }

        protected virtual void InsertDataTypeConvertTransitUnit()
        {
            if (DataType.IsNotEmpty())
            {
                ChildTransitions.Add(new TypeConvertTransitUnit
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
                ChildTransitions.Add(new WriteTransitUnit()
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
    }
}