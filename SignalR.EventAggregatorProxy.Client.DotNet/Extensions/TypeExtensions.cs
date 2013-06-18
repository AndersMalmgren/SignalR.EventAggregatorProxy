using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.Client.Extensions
{
    public static class TypeExtensions
    {
        public static string GetNameWihoutGenerics(this Type type)
        {
            return type.GetNameWihoutGenerics(t => t.Name);
        }

        public static string GetFullNameWihoutGenerics(this Type type)
        {
            return type.GetNameWihoutGenerics(t => t.FullName);
        }

        private static string GetNameWihoutGenerics(this Type type, Func<Type, string> nameProvider)
        {
            var name = nameProvider(type);
            if (type.IsGenericType)
            {
                return name.Substring(0, name.IndexOf("`", StringComparison.Ordinal));
            }

            return name;
        }
    }
}
