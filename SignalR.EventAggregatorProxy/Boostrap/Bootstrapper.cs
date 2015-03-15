using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Boostrap
{
    public static class Bootstrapper
    {
        public static void Init<TEvent>()
        {
            var locator = new Lazy<IAssemblyLocator>(() => new BuildManagerAssemblyLocator());
            GlobalHost.DependencyResolver.Register(typeof(IAssemblyLocator), () => locator.Value);

            var typeFinder = new Lazy<ITypeFinder>(() => new TypeFinder<TEvent>());
            GlobalHost.DependencyResolver.Register(typeof (ITypeFinder), () => typeFinder.Value);

            var eventProxy = new Lazy<EventProxy<TEvent>>(() => new EventProxy<TEvent>());
            GlobalHost.DependencyResolver.Register(typeof(EventProxy), () => eventProxy.Value);
            GlobalHost.DependencyResolver.Register(typeof(EventProxy<TEvent>), () => eventProxy.Value);
        }
    }
}
