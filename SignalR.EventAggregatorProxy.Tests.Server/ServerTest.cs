using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public abstract class ServerTest : Test
    {
        public override T Get<T>()
        {
            return GlobalHost.DependencyResolver.GetService(typeof(T)) as T;
        }

        public override void Register<T>(T stub)
        {
            GlobalHost.DependencyResolver.Register(typeof(T), () => stub);
        }
    }
}
