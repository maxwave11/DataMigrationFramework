using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Data.DataSources;

namespace XQ.EqDataMigrator.TargetProvider
{
    public class JsonDataSource : DataSourceBase
    {
        protected override IEnumerable<IDataObject> GetDataInternal()
        {
            var jObject = JToken.Parse(GetJson());
            var _array = jObject.SelectTokens(ActualQuery).ToList();

            //the end result shouldn't contains nested arrays. In this case need to extract all items
            //from subarrays and place them to single merged collection
            _array = _array.Select(i => (i is JArray) ? i.ToArray() : new[] { i }).SelectMany(a => a).ToList();

            if (_array.Any(i => i is JArray))
                throw new Exception("End result contains subarrays. Need to change query");

            foreach (var jToken in _array)
            {
                if (!(jToken is JObject) && !(jToken is JValue))
                    continue;

                var result = new JsonDataObject(jToken);

                yield return result;
            }
        }

        protected virtual string GetJson() {
            //Here could be your web request to recieve some JSON data
            return File.ReadAllText(@"fimx_data_01_11_20.json");
        }
    }
}
