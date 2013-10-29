using Owin;
using SignalR.EventAggregatorProxy.Boostrap;

namespace SignalR.EventAggregatorProxy.Owin
{
    public static class AppBuilderExtensions
    {
        public static void MapEventProxy<TEvent>(this IAppBuilder app)
        {
            Bootstrapper.Init<TEvent>();
            app.Map("/eventAggregation/events", subApp => subApp.Use<EventScriptMiddleware<TEvent>>());
        }
    }
}
