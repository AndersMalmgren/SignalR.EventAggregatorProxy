using System;

namespace SignalR.EventAggregatorProxy.Extensions
{
    internal static class NullableExtensions
    {
        public static T NotNull<T>(this T? source) where T : class
        {
            if (source == null) throw new NullReferenceException($"{typeof(T).FullName}: Expected reference type that is not null");

            return source;
        }
    }
}
