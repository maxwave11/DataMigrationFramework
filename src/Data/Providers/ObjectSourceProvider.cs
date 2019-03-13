using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ObjectSourceProvider : TransitionNode, IDataProvider
    {
        [XmlAttribute]
        public string From { get; set; }

        [XmlAttribute]
        public string Query { get; set; }

        public bool IsDefault { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDataSet GetDataSet(string providerQuery)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(TransitionNode parent)
        {
            base.Initialize(parent);
        }


        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var actualFrom = From.IsEmpty() ? "{VALUE}" : From;
            var sourceObject = ExpressionEvaluator.Evaluate(actualFrom, ctx);
            if (!(sourceObject is IValuesObject))
                throw new InvalidOperationException($"Source object returned by {nameof(From)} expression should implement {nameof(IValuesObject)}");

            var actualQuery = Query.Contains("{") ? (string)ExpressionEvaluator.Evaluate(Query, ctx) : Query;
            var result = GetObjects(actualQuery, (IValuesObject)sourceObject);
            return new TransitResult(result);
        }

        public IEnumerable<IValuesObject> GetObjects(string query, IValuesObject sourceObject)
        {
            List<ValuesObject> result = null;

            if (query.StartsWith("Pivot"))
            {
                var pivotExpression = query.Replace("Pivot", "").Trim();

                foreach (var pivotSubExpression in pivotExpression.Split(new[] { "and" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var chunks = pivotSubExpression.Split(new[] { "as" }, StringSplitOptions.RemoveEmptyEntries);
                    var pivotPattern = chunks[0].Trim();
                    var pivotPatternName = chunks[1].Trim();

                    var pivotColumns = FindPivotColumns(pivotPattern, sourceObject);

                    if (!pivotColumns.Any())
                    {
                        var columnNames = sourceObject.FieldNames.Select(i => $"'{i}'").Join();
                        var warningMsg = $"No pivoted columns found by pattern '{pivotPattern}'! All column names: {columnNames}";
                        TraceLine(warningMsg);
                    }

                    if (result == null)
                        result = pivotColumns.Select(i => new ValuesObject(sourceObject)).ToList();

                    for (int i = 0; i < pivotColumns.Count(); i++)
                    {
                        var pivotedObject = result[i];
                        var pivotedColumnName = pivotColumns[i];
                        pivotedObject.SetValue(pivotPatternName, sourceObject[pivotedColumnName]);
                        pivotedObject.SetValue(pivotPatternName + "Column", pivotedColumnName);
                    }
                }
                return result;
            }

            throw new NotImplementedException();
        }

            private Dictionary<string, string[]> GetPivotColumnSet(IValuesObject source, string[] patterns)
            {
                return patterns
                    .Select(pattern => new { def = pattern, Columns = FindPivotColumns(pattern, source) })
                    .ToDictionary(i => i.def, i => i.Columns);
            }

            private string[] FindPivotColumns(string columnPattern, IValuesObject source)
            {
                var regex = new Regex(columnPattern);
                return source.FieldNames.Where(f => regex.IsMatch(f.Trim())).ToArray();
            }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}