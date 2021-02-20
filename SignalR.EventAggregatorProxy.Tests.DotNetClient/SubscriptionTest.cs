using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Model;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class SubscriptionTest<TEvent> : DotNetClientTest where TEvent : class
    {
        private int subscriptionCount = 0;
        protected int exptectedSubscriptionCount = 1;

        protected virtual void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {
            
        }

        [TestInitialize]
        public void Context()
        {
            for (int i = 0; i < 2; i++)
            {
                var index = i;
                EventAggregator.Subscribe(new Mock<IHandle<TEvent>>().Object, builder => BuildConstraints(index, builder));
            }

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
        protected override void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {
            builder.For<StandardEvent>()
                .Add(new StandardEventConstraint {Id = 1});
        }
    }

    [TestClass]
    public class When_subscribing_using_fluent_constraint_synstax : SubscriptionTest<StandardEvent>
    {
        protected override void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {
            builder
                .For<StandardEvent>()
                .Add(new StandardEventConstraint {Id = 1});
        }
    }

    [TestClass]
    public class When_subscribing_to_multiple_constrained_events_of_same_type_but_different_constraint : SubscriptionTest<StandardEvent>
    {
        public When_subscribing_to_multiple_constrained_events_of_same_type_but_different_constraint()
        {
            exptectedSubscriptionCount = 2;
        }

        protected override void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {
            builder
                .For<StandardEvent>()
                .Add(new StandardEventConstraint {Id = index});
        }
    }

    [TestClass]
    public class When_subscribing_to_multiple_constrained_events_of_same_type_but_different_constraint_and_same_subscriber_issue_19 : DotNetClientTest
    {
        private int subscriptionCount = 0;

        [TestInitialize]
        public void Context()
        {
            var subscriber = new Mock<IHandle<StandardEvent>>().Object;
            EventAggregator.Subscribe(subscriber, builder => builder
                .For<StandardEvent>()
                .Add(new StandardEventConstraint {Id = 1})
                .Add(new StandardEventConstraint {Id = 2}));

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
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void It_should_throw_argument_exception()
        {
            var subscriber = new Mock<IHandle<StandardEvent>>().Object;
            EventAggregator.Subscribe(subscriber, builder => builder
                .For<StandardEvent>()
                .Add(new StandardEventConstraint {Id = 1})
                .Add(new StandardEventConstraint {Id = 1}));
        }
    }

    [TestClass]
    public class When_doing_concurrent_operations_on_subscription_queue : DotNetClientTest
    {
        private bool running = true;
        private AutoResetEvent syncTest = new AutoResetEvent(false);
        private bool failed;

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            base.ConfigureCollection(serviceCollection);

            Func<Task> callback = null;

            serviceCollection.MockTransiant<ISubscriptionThrottleHandler>(mock =>
            {
                mock.Setup(x => x.Init(It.IsAny<Func<Task>>())).Callback((Func<Task> c) => callback = c);
                mock.Setup(x => x.Throttle()).Callback(() =>
                {
                    try
                    {
                        callback().Wait();
                    }
                    catch
                    {
                        failed = true;
                        syncTest.Set();
                    }
                });
            });
        }

        protected override void OnFaultedSubscription(Exception e, IList<Subscription> subscriptions)
        {
            syncTest.Set();
            running = false;
            failed = true;
            Assert.Fail(e.Message);
        }

        [TestInitialize]
        public void Context()
        {
            var dummy = EventAggregator;

            Enumerable.Range(0, 4)
                .ForEach(i => FailIfThreadCrashes(() => Get<EventProxy>().Subscribe(new[] { new Subscription(typeof(StandardEvent), null, null) })));

            syncTest.WaitOne();
            running = false;
        }

        protected override void OnConnected()
        {
            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (s, e) => syncTest.Set();
            timer.Start();
        }

        [TestMethod]
        public void It_work_with_concurrent_operations()
        {
            Assert.IsFalse(failed);
        }

        private void FailIfThreadCrashes(Action action)
        {
            ThreadPool.QueueUserWorkItem(obj =>
            {
                while (running)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        running = false;
                        failed = true;
                        syncTest.Set();
                        Assert.Fail("Not thread safe: {0}", e.Message);
                    }
                }
            });
        }

    }

    [TestClass]
    public class When_subscribing_from_two_models_at_the_same_time : DotNetClientTest
    {
        private int subscriptionCount = 0;
        protected int exptectedSubscriptionCount = 1;

        protected virtual void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {

        }

        [TestInitialize]
        public void Context()
        {
            EventAggregator.Subscribe(new Mock<IHandle<StandardEvent>>().Object);
            EventAggregator.Subscribe(new Mock<IHandle<GenericEvent<string>>>().Object);

            reset.WaitOne();
        }

        protected override void OnSubscribe(IEnumerable<dynamic> subscriptions, bool reconnected)
        {
            subscriptionCount++;
            reset.Set();
        }

        [TestMethod]
        public virtual void It_should_only_call_server_side_subscribe_correct_number_of_times()
        {
            Assert.AreEqual(exptectedSubscriptionCount, subscriptionCount);
        }
    }

}
