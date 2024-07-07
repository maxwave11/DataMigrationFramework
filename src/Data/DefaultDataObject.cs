using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataMigrationCore.Utils;

namespace DataMigrationCore.Data
{
    public class DefaultDataObject
    {
        public object this[string name] { get => GetValue(name); set => SetValue(name, value); }
        public string[] FieldNames => _dataContainer.Keys.ToArray();

        private readonly Dictionary<string, object> _dataContainer = new();

        public object GetValue(string name)
        {
            if (!_dataContainer.TryGetValue(name, out var result))
                throw new Exception($"There is no field '{name}' in current {nameof(DefaultDataObject)}");

            return result;
        }

        public void SetValue(string name, object value)
        {
            if (name.IsEmpty())
                throw new ArgumentException($"{name} name can't be empty");

            var valueToSet = value;

            // Don't allow empty strings in source data
            // Store null always in order to simplify migration expressions
            if (value is string strValue)
            {
                if (strValue.IsEmpty())
                    valueToSet = null;
            }

            _dataContainer[name] = valueToSet;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var fieldName in FieldNames.OrderBy(i => i))
            {
                sb.AppendLine($"{fieldName}={this[fieldName]}");
            }
            return sb.ToString();
        }
    }
}
