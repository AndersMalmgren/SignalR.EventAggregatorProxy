using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;


namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public class DummyUnsubscriptionEvent : Event
    {

    }

    public abstract class UnsubscriptionTest<TEvent> : DotNetClientTest where TEvent : class
    {
        private int unsubscriptionCount = 0;
        protected int exptectedUnsubscriptionCount = 1;

        protected virtual void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {

        }

        [TestInitialize]
        public void Context()
        {
            var handlers = Enumerable.Range(0, 2).Select(i => new Mock<IHandle<TEvent>>().Object)
                .Cast<object>()
                .ToList();

            handlers.Add(new Mock<IHandle<DummyUnsubscriptionEvent>>().Object);

            for (int i = 0; i < 2; i++)
            {
                var index = i;
                EventAggregator.Subscribe(handlers[i], builder => BuildConstraints(index, builder));
            }

            reset.WaitOne();

            for (int i = 0; i < 2; i++)
                EventAggregator.Unsubscribe(handlers[i]);

            reset.WaitOne();
        }

        protected override void OnUnsubscribe(IEnumerable<object> enumerable)
        {
            unsubscriptionCount++;
            reset.Set();
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
        protected override void BuildConstraints(int index, IConstraintinfoBuilder builder)
        {
            builder
                .For<StandardEvent>()
                .Add(new StandardEventConstraint {Id = 1});
        }
    }
}
