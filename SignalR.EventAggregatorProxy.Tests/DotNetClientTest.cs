using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR.EventAggregatorProxy.Client.Bootstrap;

namespace SignalR.EventAggregatorProxy.Tests
{
    public abstract class DotNetClientTest :Test
    {
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
