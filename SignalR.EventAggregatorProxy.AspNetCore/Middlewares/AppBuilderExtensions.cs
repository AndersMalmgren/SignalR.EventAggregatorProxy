using Microsoft.AspNetCore.Builder;
using SignalR.EventAggregatorProxy.Hubs;

namespace SignalR.EventAggregatorProxy.AspNetCore.Middlewares
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseEventProxy(this IApplicationBuilder app)
        {
            return app.UseEventProxy("/eventAggregation/events");
        }

        public static IApplicationBuilder UseEventProxy(this IApplicationBuilder app, string eventsUrl)
        {
            app.Map(eventsUrl, subApp => subApp.UseMiddleware<EventScriptMiddleware>());
            return app;
        }

        public static IApplicationBuilder UseSignalREventAggregator(this IApplicationBuilder app)
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<EventAggregatorProxyHub>("/EventAggregatorProxyHub");
            });

            return app;
        }
    }
}
