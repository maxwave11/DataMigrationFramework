using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using DataMigration.Data;
using DataMigration.Utils;

namespace SampleMigrationApp.Source
{
    public class JsonDataObject : IDataObject
    {
        private readonly JToken _native;

        public object this[string name] { get => GetValue(name); set => SetValue(name, value); }

        public string Key { get; set; }
        public object Native => _native;
        public bool IsNew { get; set; }
        public uint RowNumber { get; set; }

        public string[] FieldNames
        {
            get
            {
                var jObj = _native as JObject;
                return jObj?.Properties().Select(p => p.Name).ToArray();
            }
        }

        public string Query { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public JsonDataObject(JToken nativeObject)
        {
            _native = nativeObject;
        }

        public object GetValue(string jsonPath)
        {
            object retVal = null;

            if (jsonPath.Equals("@"))
                retVal = ((JValue)_native).Value;
            else
                retVal = GetValueBySelectToken(_native, jsonPath);

            //don't allow empty strings in source data
            //always store null to keep migration expressions simple
            if (retVal is string strValue && strValue.IsEmpty())
                retVal = null;

            return retVal;
        }

        public void SetValue(string name, object value)
        {
            throw new NotImplementedException();
        }

        private object GetValueBySelectToken(JToken valuesObject, string jsonPath)
        {
            bool useNullFallback = jsonPath.EndsWith("?");
            jsonPath = jsonPath.TrimEnd('?');
            JToken queryTarget = valuesObject;
            if (jsonPath.StartsWith("^"))
            {
                jsonPath = jsonPath.TrimStart('^');
                queryTarget = FindParentByCondition(valuesObject, jsonPath);
                if (queryTarget == null)
                {
                    if (useNullFallback == false)
                        throw new Exception($"Can't find parent by condition {jsonPath}. \nObject:{valuesObject}");

                    return null;
                }
            }

            var queryResult = queryTarget.SelectToken(jsonPath);

            switch (queryResult)
            {
                case null when useNullFallback:
                    return null;
                case null:
                    throw new Exception($"Data querying by JSONPath ({jsonPath}) failed");
                case JObject _:
                    return new JsonDataObject(queryResult.Value<JObject>());
                case JValue value:
                    return value.Value;
                default:
                    return queryResult.Value<object>();
            }
        }


        /// <summary>
        ///Find parent by condition. Default implementation of JSONPath doesn't support parents querying. By this method
        // you can find first parent JObject from which query return something (not null).
        /// Method traverse up json tree and check each next parent by query
        /// </summary>
        private static JContainer FindParentByCondition(JToken valuesObject, string condition)
        {
            JContainer currentParent = valuesObject.Parent;
            while (currentParent != null)
            {
                var result = currentParent.SelectTokens(condition);

                if (result.Any())
                    return currentParent;

                currentParent = currentParent.Parent;
            }

            return null;
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