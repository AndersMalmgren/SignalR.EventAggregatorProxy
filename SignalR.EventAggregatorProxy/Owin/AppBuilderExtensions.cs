using Owin;
using SignalR.EventAggregatorProxy.Boostrap;

namespace SignalR.EventAggregatorProxy.Owin
{
    public static class AppBuilderExtensions
    {
        public static void MapEventProxy<TEvent>(this IAppBuilder app)
        {
            app.MapEventProxy<TEvent>("/eventAggregation/events");
        }

        public static void MapEventProxy<TEvent>(this IAppBuilder app, string eventsUrl)
        {
            Bootstrapper.Init<TEvent>();
            app.Map(eventsUrl, subApp => subApp.Use<EventScriptMiddleware<TEvent>>());
        }
    }
}
