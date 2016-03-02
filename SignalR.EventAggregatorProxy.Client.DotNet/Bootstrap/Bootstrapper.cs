using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR.EventAggregatorProxy.Client.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap
{
    internal static class Bootstrapper
    {
        public static DependencyResolver Create()
        {
            var resolver = new DependencyResolver();
            resolver.Register<IHubProxyFactory>(() => new HubProxyFactory());

            var subscriptionStore = new Lazy<SubscriptionStore>(() => new SubscriptionStore());
            resolver.Register<ISubscriptionStore>(() => subscriptionStore.Value);
            
            resolver.Register<ISubscriptionThrottleHandler>(() => new SubscriptionThrottleHandler());

            return resolver;
        }
    }
}
