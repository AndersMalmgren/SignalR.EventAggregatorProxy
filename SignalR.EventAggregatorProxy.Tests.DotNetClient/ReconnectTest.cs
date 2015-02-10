using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents;
using SignalR.EventAggregatorProxy.Client.Extensions;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{

    [TestClass]
    public class When_a_client_is_reconnected : DotNetClientTest
    {
        private int subscriptionCount = 0;

        [TestInitialize]
        public void Context()
        {
            var reset = new AutoResetEvent(false);

            Register<ISubscriptionStore>(new SubscriptionStore());

            var proxy = Mock<IHubProxy>();
            WhenCalling<IHubProxy>(x => x.Subscribe(Arg<string>.Is.Anything))
                .Return(new Subscription());
            var task = new Task(() => { });
            WhenCalling<IHubProxy>(x => x.Invoke(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                .Callback<string, object[]>((m, a) =>
                {
                    if (m == "subscribe")
                    {
                        subscriptionCount = (a[0] as IEnumerable<object>).Count();
                    }
                    reset.Set();

                    return true;
                })
                .Return(task);

            Mock<IHubProxyFactory>();
            Action reconnectedCallback = null;
            
            WhenCalling<IHubProxyFactory>(
                x =>
                    x.Create(Arg<string>.Is.Anything, Arg<Action<IHubConnection>>.Is.Anything, Arg<Action<IHubProxy>>.Is.Anything, Arg<Action>.Is.Anything))
                .Callback<string, Action<IHubConnection>, Action<IHubProxy>, Action>((u, c, started, reconnected) =>
                {
                    started(proxy);
                    reconnectedCallback = reconnected;
                    return true;
                })
                .Return(proxy);

            var eventAggregator = new EventAggregator<Event>()
                .Init("foo");



            var removeSubscriber = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>, IHandle<GenericEvent<string>>, IHandle<StandardEvent>>();
            var subscriberOne = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>, IHandle<GenericEvent<int>>, IHandle<StandardEvent>>();
            var subscriberTwo = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>, IHandle<GenericEvent<int>>, IHandle<StandardEvent>>();

            var thirdConstraintSubscriber = MockRepository.GenerateMock<IHandle<StandardEvent>>();

            eventAggregator.Subscribe(removeSubscriber, new[] { new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint{ Id = 2})});
            eventAggregator.Subscribe(subscriberOne, new[] { new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }) });
            eventAggregator.Subscribe(subscriberTwo, new[] { new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }) });

            eventAggregator.Subscribe(thirdConstraintSubscriber, new[] { new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 3 }) });


            reset.WaitOne();

            eventAggregator.Unsubscribe(removeSubscriber);
            reset.WaitOne();

            reconnectedCallback();

        }

        [TestMethod]
        public void It_should_resubscribe_correctly()
        {
            Assert.AreEqual(4, subscriptionCount);
        }

        public class NoneConstraintEvent : Event
        {
        }
    }
}
