using System.Web;
using System.Web.Routing;
using SignalR.EventAggregatorProxy.ScriptProxy;

namespace SignalR.EventAggregatorProxy.SystemWeb
{
    
    public class EventScriptRouteHandler<TEvent> : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ScriptHandler<TEvent>();
        }
    }

}