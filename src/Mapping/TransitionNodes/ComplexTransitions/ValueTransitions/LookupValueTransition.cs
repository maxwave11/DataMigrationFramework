using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions
{
    /// <summary>
    /// Transition which allows to get a value from some reference data set. For example, find asset and get his name by asset id or find
    /// city id by city name or by any other condition. Lookup condition determined by LookupExpr migration expression. This transition 
    /// based on LookupValueTransitUnit
    // </summary>
    //public class LookupValueTransition : TransitValueCommand
    //{
    //    public string ReplaceBeforeLookup { get; set; }
    
    //    //id категории, в которой находится элемент, на который ссылается значение этогополя (для ссылочных полей)
    //    public string LookupDataSetId { get; set; }
    
    //    /// <summary>
    //    /// Query to limit amout of objects for fetching
    //    /// </summary>
    //    public string QueryToTarget { get; set; }
    
    //    //имя ключевого поля в категории, в которой находится элемент, на который ссылается значение этогополя (для ссылочных полей)
    //    public string LookupKeyExpr { get; set; }
    
    //    //Use this propery to try to find object not by key but by other unique sequence. 
    //    //NOTE:Currently used in Avant ppe importing.
    //    public string LookupAlternativeExpr { get; set; }
    
    //    //Set this poperty to true to allow search in data sets where multiple objects can have same search lookup expression
    //    //NOTE: used only when LookupAlternativeExpr is used
    //    public bool FindFirstOccurence { get; set; }
    
    //    //имя поля ссылочного объекта, из которого подставится значение (вместо стандартного EWKey ключа этого ссылочного объекта)
    //    public string Return { get; set; }
    
    //    //указывает, к какому провайдеру будет обращаться логика Lookup для поиска нужного значения
    //    //По умолчанию - Target
    //    public string ProviderName { get; set; }
    
    //    /// <summary>
    //    /// Specify what to do if lookup value not found
    //    /// </summary>
    //    public TransitionFlow OnNotFound { get; set; } = TransitionFlow.Stop;

    //    public override void Initialize(TransitionNode parent)
    //    {
    //        if (LookupDataSetId.IsEmpty())
    //            throw new Exception($"{ nameof(LookupDataSetId)}  is required");

    //        if (LookupKeyExpr.IsEmpty() && LookupAlternativeExpr.IsEmpty())
    //            throw new Exception($"Field {nameof(LookupKeyExpr)} or {nameof(LookupAlternativeExpr)}  should be filled to search lookup object");

    //        InitializeChildTransitions();
    //        base.Initialize(parent);
    //    }

    //    protected virtual void InitializeChildTransitions()
    //    {


    //        //    var customLookupTransitions = new List<TransitionNode>();

    //        //    if (ReplaceBeforeLookup.IsNotEmpty())
    //        //        customLookupTransitions.Add(new ReplaceTransitUnit { ReplaceExpression = ReplaceBeforeLookup });

    //        //    customLookupTransitions.Add(new LookupValueTransitUnit
    //        //    {
    //        //        //LookupDataSetId = LookupDataSetId,
    //        //        LookupKeyExpr = LookupKeyExpr,
    //        //        ProviderName = ProviderName,
    //        //        OnNotFound = OnNotFound,
    //        //        QueryToTarget = QueryToTarget,
    //        //        TraceWarnings = TraceWarnings,
    //        //        LookupAlternativeExpr = LookupAlternativeExpr,
    //        //        FindFirstOccurence = FindFirstOccurence
    //        //    });

    //        //    if (Return.IsNotEmpty())
    //        //    {
    //        //        if (!Return.Contains("{"))
    //        //            Return = $"{{ VALUE[{ Return }] }}";
    //        //        customLookupTransitions.Add(new TransitUnit { Expression = Return });
    //        //    }

    //        //    Pipeline.AddRange(customLookupTransitions);
    //    }
    //}
}