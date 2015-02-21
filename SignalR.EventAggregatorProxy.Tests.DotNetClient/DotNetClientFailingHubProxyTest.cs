using System;
using System.Collections.Generic;
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
    public abstract class DotNetClientFailingHubProxyTest : Test
    {
        protected Action reconnectedCallback;
        protected EventAggregator<Event> eventAggregator;
        protected AutoResetEvent reset;
        protected Action<Exception> faultedCallback;
        protected Action connectedCallback;
        protected bool onSubscriptionErrorCalled;

        public void Setup()
        {
            reset = new AutoResetEvent(false);
            Register<ISubscriptionStore>(new SubscriptionStore());

            var proxy = Mock<IHubProxy>();
            WhenCalling<IHubProxy>(x => x.Subscribe(Arg<string>.Is.Anything))
                .Return(new Subscription());
            


            Mock<IHubProxyFactory>();

            WhenCalling<IHubProxyFactory>(x => x.Create(Arg<string>.Is.Anything, Arg<Action<IHubConnection>>.Is.Anything, Arg<Action<IHubProxy>>.Is.Anything, Arg<Action>.Is.Anything, Arg<Action<Exception>>.Is.Anything, Arg<Action>.Is.Anything))
                .Callback<string, Action<IHubConnection>, Action<IHubProxy>, Action, Action<Exception>, Action>((u, c, started, reconnected, faulted, connected) =>
                {
                    started(proxy);
                    reconnectedCallback = reconnected;
                    faultedCallback = faulted;
                    connectedCallback = connected;

                    return true;
                })
                .Return(proxy);

            eventAggregator = new EventAggregator<Event>()
                .Init("foo").OnSubscriptionError(OnSubcriptionError);

        }

        private void OnSubcriptionError(Exception arg1, IList<Client.Model.Subscription> arg2)
        {
            onSubscriptionErrorCalled = true;
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