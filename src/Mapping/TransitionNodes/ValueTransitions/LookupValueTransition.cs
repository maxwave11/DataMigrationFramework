﻿using System;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ValueTransitions
{
    /// <summary>
    /// Transition which allows to get a value from some reference data set. For example, find asset and get his name by asset id or find
    /// city id by city name or by any other condition. Lookup condition determined by LookupExpr migration expression. This transition 
    /// based on LookupValueTransitUnit
    /// </summary>
    public class LookupValueTransition : ValueTransition
    {
        [XmlAttribute]
        public string ReplaceBeforeLookup { get; set; }

        [XmlAttribute]
        //id категории, в которой находится элемент, на который ссылается значение этогополя (для ссылочных полей)
        public string LookupDataSetId { get; set; }

        [XmlAttribute]
        //имя ключевого поля в категории, в которой находится элемент, на который ссылается значение этогополя (для ссылочных полей)
        public string LookupExpr { get; set; }

        [XmlAttribute]
        //имя поля ссылочного объекта, из которого подставится значение (вместо стандартного EWKey ключа этого ссылочного объекта)
        public string Return { get; set; }

        [XmlAttribute]
        //указывает, к какому провайдеру будет обращаться логика Lookup для поиска нужного значения
        //По умолчанию - Target
        public string ProviderName { get; set; }

        public LookupValueTransition()
        {
            OnEmpty = TransitContinuation.RaiseError;
        }

        protected override void InitializeEndTransitions()
        {
            if (ReplaceBeforeLookup.IsNotEmpty())
                ChildTransitions.Add(new ReplaceTransitUnit { ReplaceRules = ReplaceBeforeLookup });

            if (LookupDataSetId.IsEmpty() || LookupExpr.IsEmpty())
                throw new Exception($"{ nameof(LookupDataSetId)} and {nameof(LookupExpr)} fields of {nameof(LookupValueTransition)} is required");

            ChildTransitions.Add(new LookupValueTransitUnit
            {
                LookupDataSetId = LookupDataSetId,
                LookupExpr = LookupExpr,
                ProviderName = ProviderName,
            });

            if (Return.IsNotEmpty())
                ChildTransitions.Add(new TransitUnit { Expression = Return , OnEmpty = OnEmpty});

            base.InitializeEndTransitions();
        }

        public override string ToString()
        {
            return base.ToString() +
                $"\n{GetIndent(5)}LookupDataSetId: {LookupDataSetId}"+
                $"\n{GetIndent(5)}LookupExpr: {LookupExpr}"+ 
                (Return.IsNotEmpty() ? $"\n{GetIndent(5)}Return: {Return}": "");
        }
    }
}