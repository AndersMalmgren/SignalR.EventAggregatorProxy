using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.Models;

namespace SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddSignalREventAggregator()
                .WithHubUrl($"{builder.HostEnvironment.BaseAddress}EventAggregatorProxyHub")
                .OnConnectionError(e => Debug.WriteLine(e.Message))
                .Build()
                .AddSingleton<IEventAggregator>(p => p.GetService<IProxyEventAggregator>())
                .AddSingleton<IEventTypeFinder, EventTypeFinder>()
                .AddScoped<EventsViewModel>()
                .AddScoped<SendMessageViewModel>();


            await builder.Build().RunAsync();
        }
    }
}
