using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.Extensions;
using SignalR.EventAggregatorProxy.Client.Model;

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

    [TestClass]
    public class When_doing_concurrent_operations_on_subscription_queue : DotNetClientTest
    {
        private bool running = true;
        private AutoResetEvent syncTest = new AutoResetEvent(false);
        private bool failed;

        [TestInitialize]
        public void Context()
        {
            var throttle = Mock<ISubscriptionThrottleHandler>();

            Action callback = null;
            WhenCalling<ISubscriptionThrottleHandler>(x => x.Init(Arg<Action>.Is.Anything)).Callback<Action>(c =>
            {
                callback = c;
                return true;
            });

            WhenCalling<ISubscriptionThrottleHandler>(x => x.Throttle())
                .WhenCalled(m =>
                {
                    try
                    {
                        callback();
                    }
                    catch
                    {
                        failed = true;
                        syncTest.Set();
                    }
                });

            Setup();
            var eventProxy = new EventProxy<Event>(eventAggregator, "foo", null, null, (e,s) =>
            {
                syncTest.Set();
                running = false;
                failed = true;
                Assert.Fail(e.Message);
            }, null);

            Enumerable.Range(0, 4)
                .ForEach(i => FailIfThreadCrashes(() => eventProxy.Subscribe(new[] {new Subscription(typeof(Event), null, null)})));

            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (s, e) => syncTest.Set();
            timer.Start();

            syncTest.WaitOne();
            running = false;
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
                while(running)
                {
                    try
                    {
                        action();
                    }
                    catch(Exception e)
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
}
