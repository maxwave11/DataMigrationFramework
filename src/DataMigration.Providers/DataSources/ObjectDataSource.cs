using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DataMigration.Data.Interfaces;
using DataMigration.Utils;

namespace DataMigration.Data.DataSources
{
    public class ObjectDataSource //INHERIT FROM DataSourceBase!
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

        //public override TransitResult ExecuteInternal(ValueTransitContext ctx)
        //{
        //    var actualFrom = From.IsEmpty() ? "{VALUE}" : From;
        //    var sourceObject = ExpressionEvaluator.Evaluate(actualFrom, ctx);
        //    if (!(sourceObject is IDataObject))
        //        throw new InvalidOperationException($"Source object returned by {nameof(From)} expression should implement {nameof(IDataObject)}");

        //    var actualQuery = ExpressionEvaluator.EvaluateString(Query, ctx);
        //    var result = GetObjects(actualQuery, (IDataObject)sourceObject);
        //    return new TransitResult(result);
        //}

        public IEnumerable<DefaultDataObject> GetObjects(string query, DefaultDataObject sourceObject)
        {
            List<DefaultDataObject> result = null;

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
                    }

                    // Restore PIVOT getting functionality
                    // if (result == null)
                    //     result = pivotColumns.Select(i => new DefaultDataObject(sourceObject)).ToList();

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

            private Dictionary<string, string[]> GetPivotColumnSet(DefaultDataObject source, string[] patterns)
            {
                return patterns
                    .Select(pattern => new { def = pattern, Columns = FindPivotColumns(pattern, source) })
                    .ToDictionary(i => i.def, i => i.Columns);
            }

            private string[] FindPivotColumns(string columnPattern, DefaultDataObject source)
            {
                var regex = new Regex(columnPattern);
                return source.FieldNames.Where(f => regex.IsMatch(f.Trim())).ToArray();
            }

       
    }
}