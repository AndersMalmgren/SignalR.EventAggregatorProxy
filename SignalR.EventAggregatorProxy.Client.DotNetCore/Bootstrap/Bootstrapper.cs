using System;
using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Options;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.ProxyEvents;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap
{
    public static class Bootstrapper
    {
        public static IOptions AddSignalREventAggregator(this IServiceCollection collection)
        {
            var options = new OptionsBuilder(collection);
            collection
                .AddSingleton<IOptionsBuilder>(options)
                .AddSingleton<IOptions>(p => p.GetRequiredService<IOptionsBuilder>())
                .AddSingleton<IHubProxyFactory, HubProxyFactory>()
                .AddSingleton<ISubscriptionStore, SubscriptionStore>()
                .AddTransient<ISubscriptionThrottleHandler, SubscriptionThrottleHandler>()
                .AddSingleton<ITypeFinder, TypeFinder>()
                .AddSingleton<EventProxy>()
                .AddSingleton<IProxyEventAggregator, ProxyEventAggregator>();

            return options;
        }
    }
}
