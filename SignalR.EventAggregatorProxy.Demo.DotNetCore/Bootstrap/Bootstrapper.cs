using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.DotNetCore.Views;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.DotNetCore.Bootstrap
{
    public class Bootstrapper : BootstrapperBase
    {
        private IServiceProvider serviceProvider;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddSignalREventAggregator()
                .WithHubUrl("http://localhost:60976/EventAggregatorProxyHub")
                .OnConnectionError(e => Debug.WriteLine(e.Message))
                .Build()
                .AddSingleton<IEventAggregator>(p => p.GetService<IProxyEventAggregator>())
                
                .AddSingleton<IWindowManager, WindowManager>()
                .AddSingleton<IEventTypeFinder, EventTypeFinder>()
                .AddTransient<MainShellViewModel>()
                .AddTransient<SendMessageViewModel>()

                .BuildServiceProvider();
        }

        protected override object GetInstance(Type service, string key)
        {
            return serviceProvider.GetService(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return serviceProvider.GetServices(service);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainShellViewModel>();
        }
    }
}
