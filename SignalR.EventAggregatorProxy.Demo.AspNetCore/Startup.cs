using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalR.EventAggregatorProxy.Boostrap;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.Owin;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services
                .AddSignalREventAggregator()
                .AddSingleton<IEventAggregator, EventAggregator>()
                .AddSingleton<IEventTypeFinder, DemoFinder>()
                .AddSingleton<IHostedService, Host>()
                .AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseEventProxy()
                .UseSignalREventAggregator()
                .UseSignalR(routes =>
                {
                    routes.MapHub<MyHub>("/myhub");

                });
        }

        private class DemoFinder : IEventTypeFinder
        {
            public IEnumerable<Type> ListEventsTypes()
            {
                return new[] {typeof(DemoEvent)};
            }
        }

        public class EventAggregator : IEventAggregator
        {
            private Func<object, Task> handler;

            public void Subscribe(Func<object, Task> handler)
            {
                this.handler = handler;
            }

            public async Task Push(DemoEvent demoEvent)
            {
                if (this.handler != null)
                    await handler(demoEvent);
            }
        }

        private class MyHub : Hub
        {

        }

        private class Host : IHostedService
        {
            private readonly IHubContext<MyHub> myHubContext;
            private EventAggregator aggregator;

            public Host(IEventAggregator aggregator, IHubContext<MyHub> myHubContext)
            {
                this.myHubContext = myHubContext;
                this.aggregator = aggregator as EventAggregator;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (true)
                {
                    await myHubContext.Clients.All.SendCoreAsync("foo", new[] {DateTime.Now.ToLongTimeString()}, cancellationToken);
                    await aggregator.Push(new DemoEvent{ Message = DateTime.Now.ToLongTimeString()});

                    await Task.Delay(1000);
                }
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
            }
        }
    }

    public class DemoEvent
    {
        public string Message { get; set; }
    }
}
