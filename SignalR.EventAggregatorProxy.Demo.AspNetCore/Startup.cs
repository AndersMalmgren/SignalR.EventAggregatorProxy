using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.AspNetCore.Middlewares;
using SignalR.EventAggregatorProxy.Boostrap;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Demo.Contracts.Constraints;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Event;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
                .AddSingleton<EventAggregation.IEventAggregator>(p => p.GetService<IEventAggregator>())
                .AddSingleton<IEventTypeFinder, EventTypeFinder>()
                .AddTransient<ConstrainedEventConstraintHandler>()
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

            app.UseMvcWithDefaultRoute();

            app.UseEventProxy()
                .UseSignalREventAggregator();
        }
    }
}
