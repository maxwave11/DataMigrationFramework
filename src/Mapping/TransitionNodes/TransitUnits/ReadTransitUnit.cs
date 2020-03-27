using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    public class ReadTransitUnit : TransitUnit
    {
        [XmlAttribute]
        public string From
        {
            get { return Expression; }
            set { Expression = value; }
        }

        public override void Initialize(TransitionNode parent)
        {
            if (!Expression.Contains("{"))
                Expression = $"{{ SRC[{ Expression }] }}";
            base.Initialize(parent);
        }
    }   
}