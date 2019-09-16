using FastMember;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XQ.DataMigration.Utils
{
    public class FastReflection
    {
        static Dictionary<Type, TypeAccessor> accessorsCache = new Dictionary<Type, TypeAccessor>();
        public static object GetValue(object dataObject, string propertyName)
        {
            return GetAccessor(dataObject.GetType())[dataObject, propertyName];
        }

        public static void SetValue(object dataObject, string propertyName, object value)
        {
            GetAccessor(dataObject.GetType())[dataObject, propertyName] = value;
        }

        private static TypeAccessor GetAccessor(Type type)
        {
            if (!accessorsCache.ContainsKey(type))
                accessorsCache[type] = TypeAccessor.Create(type);

            return accessorsCache[type];
        }

    }
}
