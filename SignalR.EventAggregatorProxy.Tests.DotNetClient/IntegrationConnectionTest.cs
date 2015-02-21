using System;
using System.Threading;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.EventAggregation;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    [TestClass]
    public class When_Connecting_With_A_Listener_Available : DotNetClientIntegrationTest
    {
        private bool connectionCalled;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [TestMethod, TestCategory("Integration")]
        public void It_Should_Call_OnConnected()
        {
            using (WebApp.Start<TestStartup>("http://localhost:12345"))
            {
                var eventAggregator = new EventAggregator<Event>().Init("http://localhost:12345").OnConnectionError(OnConnectionError).OnConnected(OnConnected);

                waitHandle.WaitOne(10000);
            }

            Assert.IsTrue(connectionCalled);
        }

        private void OnConnected()
        {
            connectionCalled = true;
            waitHandle.Set();
        }

        private void OnConnectionError(Exception ex)
        {
            waitHandle.Set();
        }
    }

    [TestClass]
    public class When_Connecting_With_A_Listener_Unavailable : DotNetClientIntegrationTest
    {
        private bool connectionErrorCalled;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [TestMethod, TestCategory("Integration")]
        public void It_Should_Call_OnConnectionError()
        {
            using (WebApp.Start<TestStartup>("http://localhost:12345"))
            {
                var eventAggregator = new EventAggregator<Event>().Init("http://localhost:2222")
                    .OnConnectionError(OnConnectionError);

                waitHandle.WaitOne(10000);
            }

            Assert.IsTrue(connectionErrorCalled);
        }

        private void OnConnectionError(Exception ex)
        {
            connectionErrorCalled = true;
            waitHandle.Set();
        }
    }

    [TestClass]
    public class When_The_server_Stops : DotNetClientIntegrationTest
    {
        private bool connectionErrorCalled;
        protected AutoResetEvent waitOnConnect = new AutoResetEvent(false);
        protected AutoResetEvent waitOnConnectionError = new AutoResetEvent(false);


        public class NoneConstraintEvent : Event
        {
        }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [TestMethod, TestCategory("Integration")]
        public void It_Should_Call_OnConnectionError()
        {
            var eventAggregator = new EventAggregator<Event>();
            var subscriberOne = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>>();
            var subscriberTwo = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>>();

            using (WebApp.Start<TestStartup>("http://localhost:1233"))
            {
                eventAggregator.Init("http://localhost:1233").OnConnectionError(OnConnectionError).OnConnected(OnConnected);

                eventAggregator.Subscribe(subscriberOne);

                waitOnConnect.WaitOne(10000);

                Assert.IsFalse(connectionErrorCalled);
            }

            Thread.Sleep(200);

            eventAggregator.Subscribe(subscriberTwo);

            waitOnConnectionError.WaitOne();

            Assert.IsTrue(connectionErrorCalled);
        }

        private void OnConnected()
        {
            waitOnConnect.Set();
        }

        private void OnConnectionError(Exception ex)
        {
            connectionErrorCalled = true;
            waitOnConnectionError.Set();
        }
    }
}
