using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.Model;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    [TestClass]
    public class When_a_connection_completes : DotNetClientTest
    {
        private bool connectedCalled;

        [TestInitialize]
        public void Context()
        {
            Setup();

            eventAggregator = new EventAggregator<Event>()
                .Init("foo")
                .OnConnected(OnConnected);

            connectedCallback();
        }

        private void OnConnected()
        {
            connectedCalled = true;
        }

        [TestMethod]
        public void It_should_call_connection_complete()
        {
            Assert.IsTrue(connectedCalled);
        }
    }

    [TestClass]
    public class When_a_subscription_sending_fails : DotNetClientTest
    {
        private bool subscriptionErrorCalled;

        [TestInitialize]
        public void Context()
        {
            Setup();

            eventAggregator = new EventAggregator<Event>()
                .Init("foo")
                .OnSubscriptionError(OnSubscriptionError);

            connectedCallback();
        }

        private void OnSubscriptionError(Exception ex, IList<Subscription> subscriptions)
        {
            subscriptionErrorCalled = true;
        }

        [TestMethod]
        public void It_should_call_subscription_erro()
        {
            Assert.IsTrue(subscriptionErrorCalled);
        }
    }

    [TestClass]
    public class When_a_connection_fails : DotNetClientTest
    {
        private bool connectionErrorCalled;

        [TestInitialize]
        public void Context()
        {
            Setup();

            eventAggregator = new EventAggregator<Event>()
                .Init("foo")
                .OnConnectionError(OnConnectionError);

            faultedCallback(new Exception());
        }

        private void OnConnectionError(Exception ex)
        {
            connectionErrorCalled = true;
        }

        [TestMethod]
        public void It_should_call_connection_error()
        {
            Assert.IsTrue(connectionErrorCalled);
        }
    }
}
