using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using XQ.DataMigration.Mapping;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public interface IValueObjectsCollecion
    {
        IEnumerable<IValuesObject> GetObjects(string query);
    }

    public class ValuesObject : IValuesObject, IValueObjectsCollecion
    {
        public object this[string name] { get => GetValue(name); set => SetValue(name, value); }
        public string[] FieldNames => _dataContainer.Keys.ToArray();
        public bool IsNew { get; }
        public string Key { get; set; }
        public object Native => this;

        private readonly Dictionary<string, object> _dataContainer = new Dictionary<string, object>();

        public ValuesObject()
        {
        }

        public ValuesObject(IValuesObject copy)
        {
            foreach (var fieldName in copy.FieldNames)
            {
                SetValue(fieldName, copy[fieldName]);
            }
        }

        public object GetValue(string name)
        {
            object result = null;
            if (!_dataContainer.TryGetValue(name, out result))
                throw new Exception($"There is no field '{name}' in current {nameof(ValuesObject)}");

            return result;
        }

        public void SetValue(string name, object value)
        {
            if (name.IsEmpty())
                throw new ArgumentException($"FromField name can't be empty");

            _dataContainer[name] = value;
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var fieldName in FieldNames)
            {
                sb.AppendLine($"{fieldName}={this[fieldName]}");
            }
            return sb.ToString();
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            return _dataContainer.Select(i=> new KeyValuePair<string, object>(i.Key, i.Value));
        }

        public override string ToString()
        {
            return $"key: { Key }, dataObject: { _dataContainer.GetType().Name.Truncate(30, "*") } ";
        }

        public void Dispose()
        {
        }

        public IEnumerable<IValuesObject> GetObjects(string query)
        {
            List<ValuesObject> result = null;

            if (query.StartsWith("Pivot"))
            {
                var pivotExpression = query.Replace("Pivot", "").Trim();

                foreach (var pivotSubExpression in pivotExpression.Split(new[] {"and"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var chunks =  pivotSubExpression.Split(new[] {"as"}, StringSplitOptions.RemoveEmptyEntries);
                    var pivotPattern = chunks[0].Trim();
                    var pivotPatternName = chunks[1].Trim();

                    var pivotColumns = FindPivotColumns(pivotPattern, this);

                    if (!pivotColumns.Any())
                    {
                        var columnNames = FieldNames.Select(i => $"'{i}'").Join();
                        var warningMsg = $"No pivoted columns found by pattern '{pivotPattern}'! All column names: {columnNames}";
                        Migrator.Current.Tracer.TraceLine(warningMsg);
                    }

                    if (result == null)
                        result = pivotColumns.Select(i => new ValuesObject(this)).ToList();

                    for (int i = 0; i < pivotColumns.Count(); i++)
                    {
                        var pivotedObject = result[i];
                        var column = pivotColumns[i];
                        pivotedObject.SetValue(pivotPatternName, _dataContainer[column]);
                        pivotedObject.SetValue(pivotPatternName + "Column", column);
                    }
                }
                return result;
            }

            throw new NotSupportedException();
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
    }
}