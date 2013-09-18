using System.Web.Routing;
using SignalR.EventAggregatorProxy.SystemWeb;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.App_Start
{
    public static class SignalRConfig
    {
        public static void Register(RouteCollection routes)
        {
            routes.MapHubs();

            /*
             * This demo is based on Ninject as dependecy resolver. If your project is small and do not have a Resolver you  can use the Resolver built into SignalR.
             * Make sure you register your event aggregator proxy after MapHubs otherwise it will override all registered types.
             */
            
            //var proxy = new Lazy<IEventAggregator>(() => new MyEventAggregatorProxy());
            //GlobalHost.DependencyResolver.Register(typeof(IEventAggregator), () => proxy.Value);

            routes.MapEventProxy<Contracts.Events.Event>();
        }
    }
}