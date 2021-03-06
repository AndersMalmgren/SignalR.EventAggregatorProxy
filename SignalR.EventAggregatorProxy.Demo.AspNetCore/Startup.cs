﻿using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using React.AspNet;
using SignalR.EventAggregatorProxy.AspNetCore.Middlewares;
using SignalR.EventAggregatorProxy.Boostrap;
using SignalR.EventAggregatorProxy.Demo.AspNetCore.CommandHandlers;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Event;




namespace SignalR.EventAggregatorProxy.Demo.AspNetCore
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddReact();
            services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName)
                .AddChakraCore();

            services.AddControllersWithViews();

            services
                .AddSignalREventAggregator()
                .AddSingleton<IEventAggregator, EventAggregator>()
                .AddSingleton<EventAggregation.IEventAggregator>(p => p.GetService<IEventAggregator>())
                .AddSingleton<IEventTypeFinder, EventTypeFinder>()
                .AddTransient<ConstrainedEventConstraintHandler>()
                .AddSignalR();

            services.AddScoped<ICommandHandler<EventCommand<StandardEvent>>, EventCommandHandler<StandardEvent>>();
            services.AddScoped<ICommandHandler<EventCommand<GenericEvent<string>>>, EventCommandHandler<GenericEvent<string>>>();
            services.AddScoped<ICommandHandler<EventCommand<ConstrainedEvent>>, EventCommandHandler<ConstrainedEvent>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseReact(config =>
            {
                // If you want to use server-side rendering of React components,
                // add all the necessary JavaScript files here. This includes
                // your components as well as all of their dependencies.
                // See http://reactjs.net/ for more information. Example:
                //config
                //  .AddScript("~/Scripts/First.jsx")
                //  .AddScript("~/Scripts/Second.jsx");

                // If you use an external build too (for example, Babel, Webpack,
                // Browserify or Gulp), you can improve performance by disabling
                // ReactJS.NET's version of Babel and loading the pre-transpiled
                // scripts. Example:
                //config
                //  .SetLoadBabel(false)
                //  .AddScriptWithoutTransform("~/Scripts/bundle.server.js");
            });


            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app
                .UseEventProxy()
                .UseSignalREventAggregator();

            app.UseEndpoints(c =>
            {
                c.MapDefaultControllerRoute();
                c.MapRazorPages();
                c.MapFallbackToFile("index.html");
            });
        }
    }
}
