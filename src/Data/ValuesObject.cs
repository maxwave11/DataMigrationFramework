using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XQ.DataMigration.Data
{
    public class ValuesObject : IValuesObject
    {
        public object this[string name] => GetValue(name);
        public string[] FieldNames => _dataContainer.Keys.ToArray();
        public bool IsNew { get; }
        public string Key { get; set; }
        public object Native => this;

        private readonly Dictionary<string, object> _dataContainer = new Dictionary<string, object>();

        public object GetValue(string name)
        {
            object result = null;
            if (!_dataContainer.TryGetValue(name, out result))
                throw new Exception($"There is no field {name} in current {nameof(ValuesObject)}");

            return result;
        }

        public void SetValue(string name, object value)
        {
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
    }
}