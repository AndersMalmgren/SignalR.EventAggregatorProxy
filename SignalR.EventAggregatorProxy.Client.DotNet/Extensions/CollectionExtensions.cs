using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.EventAggregatorProxy.Client.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if(collection != null)
            {
                foreach(var item in collection)
                {
                    action(item);
                }
            }

            return collection;
        }
    }
}
