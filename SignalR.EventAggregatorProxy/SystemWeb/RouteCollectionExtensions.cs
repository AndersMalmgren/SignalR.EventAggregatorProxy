using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.SystemWeb.Infrastructure;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.SystemWeb
{
    public static class RouteCollectionExtensions
    {
        public static void MapEventProxy<TEvent>(this RouteCollection routes)
        {
            var locator = new Lazy<ITypeFinder>(() => new TypeFinder<TEvent>());
            GlobalHost.DependencyResolver.Register(typeof(ITypeFinder), () => locator.Value);

            routes.Add(new Route(
                           "eventAggregation/events",
                           new EventScriptRouteHandler<TEvent>()
                           ));
        }
    }
}
