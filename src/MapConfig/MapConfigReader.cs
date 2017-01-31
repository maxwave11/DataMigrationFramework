using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Mapping.TransitionNodes.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions;

namespace XQ.DataMigration.MapConfig
{
    public class MapConfigReader
    {
        private readonly string _fileName;
        private readonly Dictionary<string, Type> _customElements = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _customProviders = new Dictionary<string, Type>();

        public MapConfigReader(string fileName)
        {
            _fileName = fileName;
        }

        public MapConfig Read()
        {
            XmlAttributeOverrides aor = GetCustomAttributeOverrides();
            TransitLogger.Log("Deserializing Map config file");

            using (var reader = new StreamReader(_fileName))
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
            if (!type.IsSubclassOf(typeof(ValueTransitionBase)) && !type.IsSubclassOf(typeof(ObjectTransition)))
                throw new Exception($"Types for register must be derived from {nameof(ValueTransitionBase)} or {nameof(ObjectTransition)}");

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
            var groupElementChildren = new XmlAttributes();
            groupElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
            groupElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(PivotObjectTransition), typeof(PivotObjectTransition)));
            groupElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));

            //register default object transitions
            var objectElementChildren = new XmlAttributes();
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ValueTransition), typeof(ValueTransition)));
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(LookupValueTransition), typeof(LookupValueTransition)));
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitUnit), typeof(TransitUnit)));
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(RuleUnit), typeof(RuleUnit)));
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TypeConvertTransitUnit), typeof(TypeConvertTransitUnit)));
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(LookupValueTransitUnit), typeof(LookupValueTransitUnit)));
            objectElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ReplaceTransitUnit), typeof(ReplaceTransitUnit)));

            foreach (var customTransitionType in _customElements)
            {
                if (typeof(ObjectTransition).IsAssignableFrom(customTransitionType.Value))
                {
                    groupElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
                    continue;
                }

                if (typeof(ValueTransitionBase).IsAssignableFrom(customTransitionType.Value))
                    objectElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
            }

            var providerTypes = new XmlAttributes();
            foreach (var providerType in _customProviders)
            {
                providerTypes.XmlElements.Add(new XmlElementAttribute(providerType.Key, providerType.Value));
            }

            var attribOverrides = new XmlAttributeOverrides();
            attribOverrides.Add(typeof(TransitionGroup), nameof(TransitionGroup.ObjectTransitions), groupElementChildren);
            attribOverrides.Add(typeof(ValueTransitionBase), nameof(ValueTransitionBase.ChildTransitions), objectElementChildren);
            attribOverrides.Add(typeof(ObjectTransition), nameof(ObjectTransition.ValueTransitions), objectElementChildren);
            attribOverrides.Add(typeof(DataProviderSettings), nameof(DataProviderSettings.DataProvider), providerTypes);
            return attribOverrides;
        }

        private void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new Exception("Error while mapping configuration parsing");
        }
    }
}