using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Data
{
    public class ValuesObject : IValuesObject
    {
        public object this[string name] { get => GetValue(name); set => SetValue(name, value); }
        public string[] FieldNames => _dataContainer.Keys.ToArray();
        public bool IsNew { get; }
        public uint RowNumber { get; set; }
        public string Key { get; set; }
        public object Native => this;

        public bool IsValid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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