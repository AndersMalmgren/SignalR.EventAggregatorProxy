using System;using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Boostrap
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddSignalREventAggregator(this IServiceCollection collection)
        {
            collection.AddSingleton<IAssemblyLocator, AssemblyLocator>();
            collection.AddSingleton<ITypeFinder, TypeFinder>();
            collection.AddSingleton<EventProxy>();

            return collection;
        }
    }
}
