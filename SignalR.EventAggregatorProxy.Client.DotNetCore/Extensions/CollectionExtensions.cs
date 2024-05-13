using System;
using System.Collections.Generic;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            var result = new List<T>();
            foreach(var item in collection)
            {
                action(item);
                result.Add(item);
            }
            return result;
        }
    }
}
