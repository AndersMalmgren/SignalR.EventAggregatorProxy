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
            Setup();
            for (int i = 0; i < 2; i++)
                eventAggregator.Subscribe(Mock<IHandle<TEvent>>(), GetConstraintInfos(i));

            reset.WaitOne();
        }

        protected override void OnSubscribe(IEnumerable<dynamic> subscriptions, bool reconnected)
        {
            subscriptionCount += subscriptions.Count();
            reset.Set();
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

    [TestClass]
    public class When_subscribing_to_multiple_constrained_events_of_same_type_but_different_constraint_and_same_subscriber_issue_19 : DotNetClientTest
    {
        private int subscriptionCount = 0;

        [TestInitialize]
        public void Context()
        {
            Setup();
            var subscriber = Mock<IHandle<StandardEvent>>();
            eventAggregator.Subscribe(subscriber, new[]
            {
                new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }),
                new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 2 })
            });

            reset.WaitOne();

        }

        protected override void OnSubscribe(IEnumerable<dynamic> subscriptions, bool reconnected)
        {
            subscriptionCount = subscriptions.Count();
            reset.Set();
        }


        [TestMethod]
        public void It_Should_subscribe_to_both_events()
        {
            Assert.AreEqual(2, subscriptionCount);
        }
        
    }

    [TestClass]
    public class When_subscribing_to_multiple_constrained_events_of_same_type_and_same_constraint_and_same_subscriber : DotNetClientTest
    {
        [TestInitialize]
        public void Context()
        {
            Setup();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void It_should_throw_argument_exception()
        {
            var subscriber = Mock<IHandle<StandardEvent>>();
            eventAggregator.Subscribe(subscriber, new[]
            {
                new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }),
                new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 })
            });
        }

    }
}
