using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.EventAggregatorProxy.Extensions
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }

        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            if (source is IReadOnlyCollection<T> collection) return collection;
            return source.ToList();
        }
    }
}
