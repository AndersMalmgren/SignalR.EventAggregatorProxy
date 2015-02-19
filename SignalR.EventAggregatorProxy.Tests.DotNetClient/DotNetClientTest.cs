using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.Bootstrap;
using SignalR.EventAggregatorProxy.Client.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class DotNetClientTest : Test
    {
        protected Action reconnectedCallback;
        protected EventAggregator<Event> eventAggregator;
        protected AutoResetEvent reset;

        public void Setup()
        {
            reset = new AutoResetEvent(false);
            Register<ISubscriptionStore>(new SubscriptionStore());

            var task = new Task(() => { });
            var proxy = Mock<IHubProxy>();
            WhenCalling<IHubProxy>(x => x.Subscribe(Arg<string>.Is.Anything))
                .Return(new Subscription());
            WhenCalling<IHubProxy>(x => x.Invoke(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                .Callback<string, object[]>((m, a) =>
                {
                    if(m == "subscribe")
                        OnSubscribe(a[0] as IEnumerable<dynamic>, (bool)a[1]);
                    else
                        OnUnsubscribe(a[0] as IEnumerable<dynamic>);
                    return true;
                })
                .Return(task);


            Mock<IHubProxyFactory>();

            WhenCalling<IHubProxyFactory>(x => x.Create(Arg<string>.Is.Anything, Arg<Action<IHubConnection>>.Is.Anything, Arg<Action<IHubProxy>>.Is.Anything, Arg<Action>.Is.Anything))
                .Callback<string, Action<IHubConnection>, Action<IHubProxy>, Action>((u, c, started, reconnected) =>
                {
                    started(proxy);
                    reconnectedCallback = reconnected;

                    return true;
                })
                .Return(proxy);


            eventAggregator = new EventAggregator<Event>()
                .Init("foo");
        }

        protected virtual void OnUnsubscribe(IEnumerable<object> enumerable)
        {
            reset.Set();
        }

        protected virtual void OnSubscribe(IEnumerable<dynamic> subscriptions, bool reconnected)
        {
            reset.Set();
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
