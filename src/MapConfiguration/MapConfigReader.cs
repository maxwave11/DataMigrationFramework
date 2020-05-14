namespace XQ.DataMigration.MapConfiguration
{
    //public class MapConfigReader
    //{
    //    private readonly Stream _fileStream;
    //    private readonly List<Type> _customElements = new List<Type>();
    //    private readonly List<Type> _dataProviders = new List<Type>();

    //    public MapConfigReader(string fileName)
    //    {
    //        _fileStream = new FileStream(fileName, FileMode.Open);

    //        //register default commonly used providers of source data
    //        RegisterDataProvider(typeof(CsvDataSource));
    //        RegisterDataProvider(typeof(ExcelDataSource));
    //        RegisterDataProvider(typeof(SqlDataSource));
    //    }

    //    public MapConfigReader(Stream fileStream)
    //    {
    //        _fileStream = fileStream;
    //    }

    //    public MapConfig Read()
    //    {
    //        XmlAttributeOverrides aor = GetCustomAttributeOverrides();

    //        using (var reader = new StreamReader(_fileStream))
    //        {
    //            var serializer = new XmlSerializer(typeof(MapConfig), aor);
    //            serializer.UnknownElement += Serializer_UnknownElement;

    //            var mapConfig = (MapConfig)serializer.Deserialize(reader);

    //            mapConfig.Initialize();

    //            return mapConfig;
    //        }
    //    }

    //    public void RegisterTransitElement(Type type)
    //    {
    //        var allowedBaseClasses = new[]
    //        {
    //            typeof(TransitionNode),
    //            typeof(TransitUnit),
    //            typeof(ObjectTransition)
    //        };

    //        if (!allowedBaseClasses.Any(type.IsSubclassOf))
    //            throw new Exception($"Types for register must be derived from {nameof(TransitUnit)} or {nameof(ObjectTransition)}");

    //        if (!_customElements.Contains(type))
    //            _customElements.Add(type);
    //    }

    //    public void RegisterDataProvider(Type type)
    //    {
    //        if (!typeof(IDataSource).IsAssignableFrom(type))
    //            throw new Exception($"Types for register must be derived from {nameof(IDataSource)}");

    //        if (!_dataProviders.Contains(type))
    //            _dataProviders.Add(type);
    //    }

    //    public void RegisterTargetProvider(Type type)
    //    {
    //        if (!typeof(ITargetSource).IsAssignableFrom(type))
    //            throw new Exception($"Provider types for registering must derived from {nameof(ITargetSource)}");

    //        if (!_dataProviders.Contains(type))
    //            _dataProviders.Add(type);
    //    }

    //    private XmlAttributeOverrides GetCustomAttributeOverrides()
    //    {
    //        var attribOverrides = new XmlAttributeOverrides();

    //        //register default object transitions
    //        //var objectSetElementChildren = new XmlAttributes();
    //        //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
    //        //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));
    //        //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSetTransition), typeof(ObjectSetTransition)));

    //        //register default object transitions
    //        var complexElementChildren = new XmlAttributes();
    //        //complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitionNode), typeof(ValueTransition)));
    //        //complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ComplexTransition), typeof(ValueTransition)));
         
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ValueTransition), typeof(ValueTransition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(DataReaderTransition), typeof(DataReaderTransition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(LookupValueTransition), typeof(LookupValueTransition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitUnit), typeof(TransitUnit)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(Condition), typeof(Condition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TypeConvertTransitUnit), typeof(TypeConvertTransitUnit)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ReplaceTransitUnit), typeof(ReplaceTransitUnit)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(WriteMessageUnit), typeof(WriteMessageUnit)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectTransition), typeof(ObjectTransition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(ObjectSetTransition), typeof(ObjectSetTransition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(GlobalObjectTransition), typeof(GlobalObjectTransition)));
    //        complexElementChildren.XmlElements.Add(new XmlElementAttribute(nameof(TransitionGroup), typeof(TransitionGroup)));

    //        foreach (var customTransitionType in _customElements)
    //        {
    //            if (typeof(ObjectTransition).IsAssignableFrom(customTransitionType))
    //            {
    //                complexElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Name, customTransitionType));
    //                //objectSetElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Key, customTransitionType.Value));
    //                continue;
    //            }

    //            if (typeof(TransitionNode).IsAssignableFrom(customTransitionType))
    //                complexElementChildren.XmlElements.Add(new XmlElementAttribute(customTransitionType.Name, customTransitionType));
    //        }

    //        attribOverrides.Add(typeof(ComplexTransition), nameof(ComplexTransition.ChildTransitions), complexElementChildren);
    //        attribOverrides.Add(typeof(MapConfig), nameof(ComplexTransition.ChildTransitions), complexElementChildren);


    //        var providerTypes = new XmlAttributes();
    //        foreach (var providerType in _dataProviders)
    //        {
    //            providerTypes.XmlArrayItems.Add(new XmlArrayItemAttribute(providerType));
    //        }

    //        attribOverrides.Add(typeof(MapConfig), nameof(MapConfig.DataSources), providerTypes);

    //        var nestedProviderTypes = new XmlAttributes();
    //        nestedProviderTypes.XmlElements.Add(new XmlElementAttribute(nameof(CsvDataSource), typeof(CsvDataSource)));
    //        nestedProviderTypes.XmlElements.Add(new XmlElementAttribute(nameof(ExcelDataSource), typeof(ExcelDataSource)));
    //        nestedProviderTypes.XmlElements.Add(new XmlElementAttribute(nameof(ObjectDataSource), typeof(ObjectDataSource)));

    //        attribOverrides.Add(typeof(DataReaderTransition), nameof(DataReaderTransition.DataSourceObject), nestedProviderTypes);

    //        return attribOverrides;
    //    }

    //    private void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
    //    {
    //        throw new Exception("Error while mapping configuration parsing");
    //    }
    //}
}