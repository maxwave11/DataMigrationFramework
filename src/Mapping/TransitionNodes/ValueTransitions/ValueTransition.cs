using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions
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

            base.Initialize(parent);
        }

        protected virtual void InitializeStartTransitions()
        {
            if (From.IsNotEmpty())
            {
                this.ChildTransitions.Add(new TransitUnit { Expression = From });
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
                this.ChildTransitions.Add(new ReplaceTransitUnit { ReplaceRules = Replace });
            }

            if (DataType.IsNotEmpty())
            {
                this.ChildTransitions.Add(new TypeConvertTransitUnit { DataType = DataType, DataTypeFormats = DataTypeFormat });
            }

            if (To.IsNotEmpty())
            {
                this.ChildTransitions.Add(new WriteTransitUnit() { Expression = To});
            }
        }

        public override string ToString()
        {
            var info = base.ToString() +
               $"\n    From: { From }" +
               $"\n    To: { To }" +
               (Replace.IsNotEmpty() ? $"\n    Replace: { Replace }" : "");
            return info;
        }
    }
}