using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using SignalR.EventAggregatorProxy.Demo.MVC4.App_Start;
using SignalR.EventAggregatorProxy.Demo.MVC4.EventConstraintHandlers;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Owin;

[assembly: OwinStartup(typeof(SignalRConfig))]
namespace SignalR.EventAggregatorProxy.Demo.MVC4.App_Start
{
    public static class SignalRConfig
    {
        public static void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            var eventAggregator = new Lazy<Caliburn.Micro.IEventAggregator>(() => new Caliburn.Micro.EventAggregator());
            GlobalHost.DependencyResolver.Register(typeof(Caliburn.Micro.IEventAggregator), () => eventAggregator.Value);

            var proxy = new Lazy<IEventAggregator>(() => new EventProxy.EventAggregatorProxy(eventAggregator.Value));
            GlobalHost.DependencyResolver.Register(typeof(IEventAggregator), () => proxy.Value);

            var constraint = new Lazy<ConstrainedEventConstraintHandler>(() => new ConstrainedEventConstraintHandler());
            GlobalHost.DependencyResolver.Register(typeof(ConstrainedEventConstraintHandler), () => constraint.Value);

            app.MapEventProxy<Contracts.Events.Event>();


        }
    }
}