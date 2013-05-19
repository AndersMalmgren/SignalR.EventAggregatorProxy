using System;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.Boostrap;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.SystemWeb
{
    public static class RouteCollectionExtensions
    {
        public static void MapEventProxy<TEvent>(this RouteCollection routes)
        {
            Bootstrapper.Init<TEvent>();

            routes.Add(new Route(
                           "eventAggregation/events",
                           new RouteValueDictionary(),
                           new RouteValueDictionary() {{"controller", string.Empty}},
                           new EventScriptRouteHandler<TEvent>()));
        }
    }
}
