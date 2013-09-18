using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Ninject;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Demo.DotNet.Views;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.DotNet.Bootstrap
{
    public class Bootstrapper : Bootstrapper<MainShellViewModel>
    {
        private readonly IKernel kernel = new StandardKernel();

        protected override void Configure()
        {
            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            kernel
                .Bind<IEventAggregator, IEventAggregator<Event>>()
                .ToConstant(new EventAggregator<Event>().Init("http://localhost:2336/"));
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }
    }
}
