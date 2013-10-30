using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class SubscriptionTest<TEvent> : DotNetClientTest where TEvent : class
    {
        private int subscriptionCount = 0;
        protected int exptectedSubscriptionCount = 1;

        protected virtual IEnumerable<IConstraintInfo> GetConstraintInfos(int index)
        {
            return new List<IConstraintInfo>();
        }
            
            [TestInitialize]
        public void Context()
        {
            var reset = new AutoResetEvent(false);

            Register<ISubscriptionStore>(new SubscriptionStore());

            var proxy = Mock<IHubProxy>();
            WhenCalling<IHubProxy>(x => x.Subscribe(Arg<string>.Is.Anything))
                .Return(new Subscription());
            WhenCalling<IHubProxy>(x => x.Invoke(Arg<string>.Is.Equal("subscribe"), Arg<object[]>.Is.Anything))
                .Callback<string, object[]>((m, a) =>
                {
                    subscriptionCount += (a[0] as IEnumerable<dynamic>).Count();
                    reset.Set();
                    return true;
                })
                .Return(null);

            Mock<IHubProxyFactory>();

            WhenCalling<IHubProxyFactory>(x => x.Create(Arg<string>.Is.Anything, Arg<Action<IHubConnection>>.Is.Anything, Arg<Action<IHubProxy>>.Is.Anything))
                .Callback<string, Action<IHubConnection>, Action<IHubProxy>>((u, c, started) =>
                {
                    started(proxy);
                    return true;
                })
                .Return(proxy);

            var eventAggregator = new EventAggregator<Event>()
                .Init("foo");

            for (int i = 0; i < 2; i++)
                eventAggregator.Subscribe(Mock<IHandle<TEvent>>(), GetConstraintInfos(i));

            reset.WaitOne();
        }

        [TestMethod]
        public virtual void It_should_only_call_server_side_subscribe_correct_number_of_times()
        {
            Assert.AreEqual(exptectedSubscriptionCount, subscriptionCount);
        }
    }

    [TestClass]
    public class When_subscribing_to_multiple_events_of_same_type : SubscriptionTest<StandardEvent>
    {

    }

    [TestClass]
    public class When_subscribing_to_multiple_generic_events_of_same_type : SubscriptionTest<GenericEvent<string>>
    {

    }

    [TestClass]
    public class When_subscribing_to_multiple_constrained_events_of_same_type : SubscriptionTest<StandardEvent>
    {
        protected override IEnumerable<IConstraintInfo> GetConstraintInfos(int index)
        {
            return new[] {new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint {Id = 1})};
        }
    }

    [TestClass]
    public class When_subscribing_to_multiple_constrained_events_of_same_type_but_different_constraint : SubscriptionTest<StandardEvent>
    {
        public When_subscribing_to_multiple_constrained_events_of_same_type_but_different_constraint()
        {
            exptectedSubscriptionCount = 2;
        }

        protected override IEnumerable<IConstraintInfo> GetConstraintInfos(int index)
        {
            return new[] { new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = index }) };
        }
    }
}
