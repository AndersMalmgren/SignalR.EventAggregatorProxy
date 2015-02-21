using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using SignalR.EventAggregatorProxy.Client.Bootstrap;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Owin;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    [TestClass]
    public class DotNetClientIntegrationTest : Test
    {
        protected AutoResetEvent waitHandle = new AutoResetEvent(false);

        public class StubAssemblyLocator : IAssemblyLocator
        {
            public IEnumerable<Assembly> GetAssemblies()
            {
                yield return GetType().Assembly;
            }
        }

        public virtual void Setup()
        {

            var locator = new Lazy<IAssemblyLocator>(() => new StubAssemblyLocator());
            GlobalHost.DependencyResolver.Register(typeof (IAssemblyLocator), () => locator.Value);

            var typeFinder = new Lazy<ITypeFinder>(() => new TypeFinder<Event>());
            GlobalHost.DependencyResolver.Register(typeof (ITypeFinder), () => typeFinder.Value);
        }

        public class TestStartup
        {
            public void Configuration(IAppBuilder app)
            {
                app.MapSignalR();

                app.Map("/eventAggregation/events", subApp => subApp.Use<EventScriptMiddleware<Event>>());
            }
        }

        protected override void Reset()
        {
        }

        public override T Get<T>()
        {
            return DependencyResolver.Global.Get<T>();
        }

        public override void Register<T>(T stub)
        {
            DependencyResolver.Global.Register(() => stub);
        }
    }
}
