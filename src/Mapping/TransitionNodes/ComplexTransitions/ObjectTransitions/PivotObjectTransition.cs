using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    public class PivotColumnDefinition
    {
        [XmlAttribute]
        public string HeaderPattern { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool IsHeaderValue { get; set; }
    }

    public class PivotObjectTransition : ObjectTransition
    {
        public List<PivotColumnDefinition> PivotColumnDefinitions { get; set; }

        public override void Initialize(TransitionNode parent)
        {
            if (PivotColumnDefinitions?.Any() != true)
                throw new Exception($"{nameof(PivotColumnDefinitions)} element is required for { nameof(PivotObjectTransition)} type");

            if (PivotColumnDefinitions.Any(i=>i.Name.IsEmpty()))
                throw new Exception($"{nameof(PivotColumnDefinition.Name)} attribute is required for { nameof(PivotColumnDefinition)} element");

            if (PivotColumnDefinitions.Any(i => i.HeaderPattern.IsEmpty()))
                throw new Exception($"{nameof(PivotColumnDefinition.HeaderPattern)} attribute is required for { nameof(PivotColumnDefinition)} element");

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext transitContext)
        {
            throw new NotImplementedException();
            //var retVal = new List<IValuesObject>();
            //var pivotColumnsSet = GetPivotColumnSet(source);
            //ValidatePivotColumns(pivotColumnsSet);
            //var allPivotColumns = pivotColumnsSet.SelectMany(i => i.Value).ToArray();

            ////go through all pivoted column set 
            //for (int columnSetIndex = 0; columnSetIndex < pivotColumnsSet.First().Value.Count(); columnSetIndex++)
            //{
            //    var valuesObject = new ValuesObject();
            //    var unpivotedColumnNames = source.FieldNames.Where(f => !allPivotColumns.Contains(f)).ToList();
            //    //copy all unpivoted (scalar, not involved in pivoting logic) columns to new values object
            //    unpivotedColumnNames.ForEach(colName => valuesObject.SetValue(colName, source[colName]));


            //    //add pivoted column header values
            //    PivotColumnDefinitions.Where(i => i.IsHeaderValue).ToList()
            //        .ForEach(def => valuesObject.SetValue(def.Name, pivotColumnsSet[def][columnSetIndex]));

            //    //add pivoted columns with values
            //    PivotColumnDefinitions.Where(i => !i.IsHeaderValue).ToList().ForEach(def => valuesObject.SetValue(def.Name, source[pivotColumnsSet[def][columnSetIndex]]));
            //    var objects = base.Transit(valuesObject);
            //    if (objects != null)
            //        retVal.AddRange(objects);
            //}

            //return retVal;
        }

        private void ValidatePivotColumns(Dictionary<PivotColumnDefinition,string[]> pivotColumnsSet)
        {
            if (pivotColumnsSet.Any(i => !i.Value.Any()))
            {
                var nofFound = pivotColumnsSet.First(i => !i.Value.Any()).Key;
                throw new Exception($"Colums with header pattern ='{nofFound.HeaderPattern}' not found");
            }
            var firstCount = pivotColumnsSet.First().Value.Count();
            if (pivotColumnsSet.Any(i => i.Value.Count() != firstCount))
                throw new Exception($"Pivot columns count not equals");
        }


        private Dictionary<PivotColumnDefinition, string[]> GetPivotColumnSet(IValuesObject source)
        {
            return PivotColumnDefinitions
                .Select(def => new {def, Columns = FindPivotColumns(def.HeaderPattern, source)})
                .ToDictionary(i => i.def, i => i.Columns);
        }

        private string[] FindPivotColumns(string headerPattern, IValuesObject source)
        {
            var regex = new Regex(headerPattern);
            return source.FieldNames.Where(f => regex.IsMatch(f)).ToArray();
        }

    }
}