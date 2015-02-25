using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Ninject;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Demo.DotNet.Views;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.DotNet.Bootstrap
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly IKernel kernel = new StandardKernel();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            kernel
                .Bind<IEventAggregator, IEventAggregator<Event>>()
                .ToConstant(new EventAggregator<Event>()
                .OnConnectionError(e => Debug.WriteLine(e.Message))
                .Init("http://localhost:2336/"));
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainShellViewModel>();
        }
    }
}
