using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;

namespace XQ.DataMigration.MapConfig
{
    public class MapConfigReader
    {
        private readonly Stream _fileStream;
        private readonly Dictionary<string, Type> _customElements = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _customProviders = new Dictionary<string, Type>();

        public MapConfigReader(string fileName)
        {
            _fileStream = new FileStream(fileName, FileMode.Open);
        }

        public MapConfigReader(Stream fileStream)
        {
            _fileStream = fileStream;
        }

        public MapConfig Read()
        {
            XmlAttributeOverrides aor = GetCustomAttributeOverrides();

            using (var reader = new StreamReader(_fileStream))
            {
                var serializer = new XmlSerializer(typeof(MapConfig), aor);
                serializer.UnknownElement += Serializer_UnknownElement;

                var mapConfig = (MapConfig)serializer.Deserialize(reader);

                mapConfig.Initialize();

                return mapConfig;
            }
        }

        public void RegisterTransitElement(Type type)
        {
            var allowedBaseClasses = new []
            {
                typeof(TransitionNode), 
                typeof(TransitUnit), 
                typeof(ObjectTransition)
            };
            
            if (!allowedBaseClasses.Any(type.IsSubclassOf))
                throw new Exception($"Types for register must be derived from {nameof(TransitUnit)} or {nameof(ObjectTransition)}");

            _customElements[type.Name] = type;
        }

        public void RegisterSourceProvider(Type type)
        {
            if (!typeof(ISourceProvider).IsAssignableFrom(type))
                throw new Exception($"Types for register must be derived from {nameof(ISourceProvider)}");

            _customProviders[type.Name] = type;
        }

        public void RegisterTargetProvider(Type type)
        {
            if (!typeof(ITargetProvider).IsAssignableFrom(type))
                throw new Exception($"Types for register must be derived from {nameof(ITargetProvider)}");

            _customProviders[type.Name] = type;
        }

        private XmlAttributeOverrides GetCustomAttributeOverrides()
        {
            //register default object transitions
            //var objectSetElementChildren = new XmlAttributes();
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(PivotObjectTransition), typeof(PivotObjectTransition)));
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSetTransition), typeof(ObjectSetTransition)));


            //register default object transitions
            var complexElementChildren = new XmlAttributes();
            //complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitionNode), typeof(ValueTransition)));
            //complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ComplexTransition), typeof(ValueTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ValueTransition), typeof(ValueTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(LookupValueTransition), typeof(LookupValueTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitUnit), typeof(TransitUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ConditionUnit), typeof(ConditionUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TypeConvertTransitUnit), typeof(TypeConvertTransitUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ReplaceTransition), typeof(ReplaceTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(WriteMessageUnit), typeof(WriteMessageUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSetTransition), typeof(ObjectSetTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(PivotObjectTransition), typeof(PivotObjectTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));

            foreach (var customTransitionType in _customElements)
            {
                if (typeof(ObjectTransition).IsAssignableFrom(customTransitionType.Value))
                {
                    complexElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
                    //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
                    continue;
                }

                if (typeof(TransitionNode).IsAssignableFrom(customTransitionType.Value))
                    complexElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
            }

            var providerTypes = new XmlAttributes();
            foreach (var providerType in _customProviders)
            {
                providerTypes.XmlElements.Add(new XmlElementAttribute(providerType.Key, providerType.Value));
            }

            var attribOverrides = new XmlAttributeOverrides();
            attribOverrides.Add(typeof(ComplexTransition), nameof(ComplexTransition.ChildTransitions), complexElementChildren);
            //attribOverrides.Add(typeof(ObjectSetTransition), nameof(ObjectSetTransition.ObjectTransition), objectSetElementChildren);
            attribOverrides.Add(typeof(DataProviderSettings), nameof(DataProviderSettings.DataProvider), providerTypes);
            return attribOverrides;
        }

        private void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new Exception("Error while mapping configuration parsing");
        }
    }
}