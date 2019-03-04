using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{

    [TestClass]
    public class When_a_client_is_reconnected : DotNetClientTest
    {
        private int subscriptionCount = 0;
        private int connectedCount;

        [TestInitialize]
        public async Task Context()
        {
            var removeSubscriber = new Mock<IHandle<NoneConstraintEvent>>().As<IHandle<GenericEvent<string>>>().As<IHandle<StandardEvent>>().Object; ;
            var subscriberOne = new Mock<IHandle<NoneConstraintEvent>>().As<IHandle<GenericEvent<int>>>().As<IHandle<StandardEvent>>().Object;
            var subscriberTwo = new Mock<IHandle<NoneConstraintEvent>>().As<IHandle<GenericEvent<int>>>().As<IHandle<StandardEvent>>().Object;

            var thirdConstraintSubscriber = new Mock<IHandle<StandardEvent>>().Object;

            EventAggregator.Subscribe(removeSubscriber, builder => builder.Add<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 2 }));
            EventAggregator.Subscribe(subscriberOne, builder => builder.Add<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }));
            EventAggregator.Subscribe(subscriberTwo, builder => builder.Add<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 1 }));

            EventAggregator.Subscribe(thirdConstraintSubscriber, builder => builder.Add<StandardEvent, StandardEventConstraint>(new StandardEventConstraint { Id = 3 }));


            reset.WaitOne();

            EventAggregator.Unsubscribe(removeSubscriber);
            reset.WaitOne();

            await reconnectedCallback();
        }

       
        protected override void OnConnected()
        {
            connectedCount++;
        }


        protected override void OnSubscribe(IEnumerable<dynamic> subscriptions, bool reconnected)
        {
            subscriptionCount = subscriptions.Count();
            reset.Set();
        }

        [TestMethod]
        public void It_should_resubscribe_correctly()
        {
            Assert.AreEqual(4, subscriptionCount);
            Assert.AreEqual(2, connectedCount);
        }

        public class NoneConstraintEvent : Event
        {
        }
    }
}
