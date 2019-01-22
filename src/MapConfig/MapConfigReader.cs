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
        private readonly List<Type> _customElements = new List<Type>();
        private readonly List<Type> _dataProviders = new List<Type>();

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

            if (!_customElements.Contains(type))
                _customElements.Add(type);
        }

        public void RegisterSourceProvider(Type type)
        {
            if (!typeof(ISourceProvider).IsAssignableFrom(type))
                throw new Exception($"Types for register must be derived from {nameof(ISourceProvider)}");

            if(!_dataProviders.Contains(type))
                _dataProviders.Add(type);
        }

        public void RegisterTargetProvider(Type type)
        {
            if (!typeof(ITargetProvider).IsAssignableFrom(type))
                throw new Exception($"Provider types for registering must derived from {nameof(ITargetProvider)}");

            if (!_dataProviders.Contains(type))
                _dataProviders.Add(type);
        }

        private XmlAttributeOverrides GetCustomAttributeOverrides()
        {
            var attribOverrides = new XmlAttributeOverrides();

            //register default object transitions
            //var objectSetElementChildren = new XmlAttributes();
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));
            //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSetTransition), typeof(ObjectSetTransition)));

            //register default object transitions
            var complexElementChildren = new XmlAttributes();
            //complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitionNode), typeof(ValueTransition)));
            //complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ComplexTransition), typeof(ValueTransition)));
         
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ValueTransition), typeof(ValueTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ForEachTransition), typeof(ForEachTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(LookupValueTransition), typeof(LookupValueTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitUnit), typeof(TransitUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ConditionUnit), typeof(ConditionUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TypeConvertTransitUnit), typeof(TypeConvertTransitUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ReplaceTransition), typeof(ReplaceTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(WriteMessageUnit), typeof(WriteMessageUnit)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSetTransition), typeof(ObjectSetTransition)));
            complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));

            foreach (var customTransitionType in _customElements)
            {
                if (typeof(ObjectTransition).IsAssignableFrom(customTransitionType))
                {
                    complexElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Name, customTransitionType));
                    //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
                    continue;
                }

                if (typeof(TransitionNode).IsAssignableFrom(customTransitionType))
                    complexElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Name, customTransitionType));
            }

            attribOverrides.Add(typeof(ComplexTransition), nameof(ComplexTransition.ChildTransitions), complexElementChildren);


            var providerTypes = new XmlAttributes();
            foreach (var providerType in _dataProviders)
            {
                providerTypes.XmlArrayItems.Add(new XmlArrayItemAttribute(providerType));
            }

            attribOverrides.Add(typeof(MapConfig), nameof(MapConfig.DataProviders), providerTypes);

            var nestedProviderTypes = new XmlAttributes();
            nestedProviderTypes.XmlElements.Add(new XmlElementAttribute(nameof(CsvProvider), typeof(CsvProvider)));
            nestedProviderTypes.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSourceProvider), typeof(ObjectSourceProvider)));

            attribOverrides.Add(typeof(ForEachTransition), nameof(ForEachTransition.DataProvider), nestedProviderTypes);

            return attribOverrides;
        }

        private void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new Exception("Error while mapping configuration parsing");
        }
    }
}