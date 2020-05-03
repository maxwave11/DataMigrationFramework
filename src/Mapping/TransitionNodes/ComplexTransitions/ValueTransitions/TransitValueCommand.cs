using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions
{

    public class TransitValueCommand : ComplexTransition
    {
        public MigrationExpression From { get; set; }

        public string Replace { get; set; }

        public string DataType { get; set; }

        public string DecimalSeparator { get; set; } = ".";

        public string DataTypeFormat { get; set; }

        public string Format { get; set; }

        public MigrationExpression To { get; set; }

        public TransitValueCommand() 
        {
            Color = ConsoleColor.Green;
        }

        public override void Initialize(TransitionNode parent)
        {
            InitializeChildTransitions();
            base.Initialize(parent);
        }
       
        protected virtual void InitializeChildTransitions()
        {
            Pipeline.Clear();

            Pipeline.Insert(0, new TransitUnit() { Expression = From, OnError = this.OnError });

            if (Replace.IsNotEmpty())
                Pipeline.Add(new ReplaceTransitUnit { ReplaceExpression = Replace, OnError = this.OnError });

            if (DataType.IsNotEmpty())
                Pipeline.Add(new TypeConvertTransitUnit { 
                    DataType = DataType,
                    DataTypeFormats = DataTypeFormat,
                    DecimalSeparator = DecimalSeparator,
                    OnError = this.OnError
                });

            //WriteTransitUnit must be always last transtion in ChildTransitions collection
            if (To != null)
                Pipeline.Add(new WriteTransitUnit() { Expression = To, OnError = this.OnError });
        }

        protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            attributes = $"From: {From}, To: {To}";
            base.TraceStart(ctx, attributes);
        }


        public static implicit operator TransitValueCommand(string expression)
        {
            return new TransitValueCommand() { From = expression };
        }
    }
}