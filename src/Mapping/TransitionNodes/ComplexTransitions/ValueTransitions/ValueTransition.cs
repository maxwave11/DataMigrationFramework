using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfig;
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
        public string To { get; set; }

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

        public override void Initialize(TransitionNode parent)
        {

            if (ChildTransitions == null)
                ChildTransitions = new List<TransitionNode>();

            var userDefinedTransitions = ChildTransitions.ToList();
            ChildTransitions?.Clear();
            InitializeStartTransitions();
            InitializeUserDefinedTransitions(userDefinedTransitions);
            InitializeEndTransitions();
            Color = ConsoleColor.Green;
            base.Initialize(parent);
        }

        protected virtual void InitializeStartTransitions()
        {
            if (From.IsNotEmpty())
            {
                this.ChildTransitions.Add(new ReadTransitUnit(){ From = From , OnError = this.OnError});
            }
        }

        protected virtual void InitializeUserDefinedTransitions(IEnumerable<TransitionNode> userDefinedTransitions)
        {
            this.ChildTransitions.AddRange(userDefinedTransitions);
        }

       

        protected virtual void InitializeEndTransitions()
        {
            if (Replace.IsNotEmpty())
            {
                this.ChildTransitions.Add(new ReplaceTransition
                {
                    ReplaceRules = Replace,
                    OnError = this.OnError
                });
            }

            if (DataType.IsNotEmpty())
            {
                this.ChildTransitions.Add(new TypeConvertTransitUnit
                {
                    DataType = DataType,
                    DataTypeFormats = DataTypeFormat,
                   // DecimalSeparator = DecimalSeparator,
                    OnError = this.OnError
                });
            }

            if (To.IsNotEmpty())
            {
                this.ChildTransitions.Add(new WriteTransitUnit()
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