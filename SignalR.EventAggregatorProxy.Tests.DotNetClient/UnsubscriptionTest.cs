using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.Extensions;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public class DummyUnsubscriptionEvent : Event
    {
        
    }

    public abstract class UnsubscriptionTest<TEvent> : DotNetClientTest where TEvent : class
    {
        private int unsubscriptionCount = 0;
        protected int exptectedUnsubscriptionCount = 1;

        protected virtual IEnumerable<IConstraintInfo> GetConstraintInfos(int index)
        {
            return new List<IConstraintInfo>();
        }

        [TestInitialize]
        public void Context()
        {
            var reset = new AutoResetEvent(false);

            Register<ISubscriptionStore>(new SubscriptionStore());

            var task = new Task(() => { });
            var proxy = Mock<IHubProxy>();
            WhenCalling<IHubProxy>(x => x.Subscribe(Arg<string>.Is.Anything))
                .Return(new Subscription());
            
            WhenCalling<IHubProxy>(x => x.Invoke(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                .Callback<string, object[]>((m, a) =>
                {
                    if (m == "unsubscribe")
                    {
                        unsubscriptionCount++;
                    }

                    reset.Set();
                    return true;
                })
                .Return(task);

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

            var handlers = Enumerable.Range(0, 2).Select(i => Mock<IHandle<TEvent>>())
                .Cast<object>()
                .ToList();
            
            handlers.Add(Mock<IHandle<DummyUnsubscriptionEvent>>());
            
            for (int i = 0; i < 2; i++)
                eventAggregator.Subscribe(handlers[i], GetConstraintInfos(i));
            
            reset.WaitOne();

            for (int i = 0; i < 2; i++)
                eventAggregator.Unsubscribe(handlers[i]);

            reset.WaitOne();
        }

        [TestMethod]
        public virtual void It_should_only_call_server_side_unsubscribe_correct_number_of_times()
        {
            Assert.AreEqual(exptectedUnsubscriptionCount, unsubscriptionCount);
        }
    }

    [TestClass]
    public class When_unsubscribing_to_a_event_that_is_subscribed_multiple_times : UnsubscriptionTest<StandardEvent>
    {
        
    }

    [TestClass]
    public class When_unsubscribing_to_a_geneic_event_that_is_subscribed_multiple_times : UnsubscriptionTest<GenericEvent<string>>
    {

    }

    [TestClass]
    public class When_unsubscribing_to_a_constrained_event_that_is_subscribed_multiple_times : UnsubscriptionTest<StandardEvent>
    {
        protected override IEnumerable<IConstraintInfo> GetConstraintInfos(int index)
        {
            return new[] { new ConstraintInfo<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }) };
        }
    }
}
