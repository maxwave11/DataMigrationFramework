using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DataMigration.Data;
using DataMigration.Utils;

namespace XQTargetProvider
{
    public class XqTargetObject : IDataObject
    {
        private readonly object _dataObject;
        private bool _isEmpty;
        
        public bool IsNew { get; set; }
        public uint RowNumber { get; set; }
        
        public string[] FieldNames => throw new NotImplementedException();

        public string Key { get; set; }
        object IDataObject.Native => _dataObject;

        public string Query { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        object IDataObject.this[string name] { get => GetValue(name); set => SetValue(name,value); }

        public XqTargetObject(object dataObject, bool isNew = false)
        {
            _dataObject = dataObject;
            _isEmpty = IsNew = isNew;
        }

        public string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("IsNew = " + IsNew);
            sb.AppendLine("Key = " + Key);
            var properties = _dataObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var propInfo in properties.OrderBy(i=>i.Name))
            {
                string valueStr;
                try
                {
                    var value = propInfo.GetValue(_dataObject);

                    //don't show enumerables
                    // if (value is Enumerable)
                    //     continue;

                    //show string values with quotations
                    if (value is string)
                        value = $"'{value}'";

                    valueStr = value?.ToString().Truncate(50);
                }
                catch
                {
                    valueStr = "error while trying to get value!";
                }

                sb.AppendLine($"{propInfo.Name} = {valueStr}");
            }
            return sb.ToString();
        }

        public object GetValue(string fieldName)
        {
            //code for getting value from native object
            return "dummy value";
        }

        public void SetValue(string fieldName, object value)
        {
            //code for setting value for native object
            return;
        }

        public bool IsEmpty()
        {
            return _isEmpty;
        }

        public T Native<T>() where T : class
        {
            return (T) _dataObject;
        }

        public object Native()
        {
            return _dataObject;
        }

        public override string ToString()
        {
            return $"key: { Key }, dataObject: { Native().GetType().Name.Truncate(30, "*") } ";
        }
    }
}
