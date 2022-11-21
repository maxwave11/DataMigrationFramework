using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataMigration.Utils;

namespace DataMigration.Data
{
    public sealed class DefaultDataObject : IDataObject
    {
        public object this[string name] { get => GetValue(name); set => SetValue(name, value); }
        public string[] FieldNames => _dataContainer.Keys.ToArray();
        public bool IsNew { get; set; }
        public uint RowNumber { get; set; }
        public string Key { get; set; }
        
        public string Query { get; set; }

        private readonly Dictionary<string, object> _dataContainer = new Dictionary<string, object>();

        public DefaultDataObject()
        {
        }

        public DefaultDataObject(DefaultDataObject copy)
        {
            foreach (var fieldName in copy.FieldNames)
            {
                SetValue(fieldName, copy[fieldName]);
            }
        }

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

        public bool IsEmpty()
        {
            return _dataContainer.Count == 0;
        }

        public string GetInfo()
        {
            var sb = new StringBuilder();
            foreach (var fieldName in FieldNames.OrderBy(i=>i))
            {
                sb.AppendLine($"{fieldName}={this[fieldName]}");
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return $"key: { Key }, dataObject: { _dataContainer.GetType().Name.Truncate(30, "*") } ";
        }
    }
}