using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Client.EventAggregation;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    [TestClass]
    public class When_subscriptions_are_registered_and_HubProxy_returns_an_exception : DotNetClientFailingHubProxyTest
    {

        [TestInitialize]
        public void Context()
        {
            Setup();

            var task = new Task(() => { });

            WhenCalling<IHubProxy>(x => x.Invoke(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                .Callback<string, object[]>((m, a) =>
                {
                    throw new Exception(m + " method failed on hub / hubproxy");
                })
                .Return(task);

        }

        [TestMethod]
        public void Then_OnSubscriptionError_Should_Be_Called()
        {
            var subscriberOne = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>>();

            eventAggregator.Subscribe(subscriberOne);

            reset.WaitOne();

            Assert.IsTrue(onSubscriptionErrorCalled);
        }

        public class NoneConstraintEvent : Event
        {
        }
    }

    [TestClass]
    public class When_subscriptions_are_unregistered_and_HubProxy_returns_an_exception : DotNetClientFailingHubProxyTest
    {

        [TestInitialize]
        public void Context()
        {
            Setup();

            var task = new Task(() => { });

            WhenCalling<IHubProxy>(x => x.Invoke(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                .Callback<string, object[]>((m, a) =>
                {
                    if (m == "subscribe")
                        return true;

                    throw new Exception(m + " method failed on hub / hubproxy");
                })
                .Return(task);
        }

        [TestMethod]
        public void Then_OnSubscriptionError_Should_Be_Called()
        {
            var subscriberOne = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>>();

            eventAggregator.Subscribe(subscriberOne);

            bool failed = false; 

            try
            {
                eventAggregator.Unsubscribe(subscriberOne);
            }
            catch (Exception)
            {
                failed = true;
            }

            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void Then_OnSubscriptionError_Should_Not_Be_Called()
        {
            var subscriberOne = MockRepository.GenerateMock<IHandle<NoneConstraintEvent>>();

            eventAggregator.Subscribe(subscriberOne);

            try
            {
                eventAggregator.Unsubscribe(subscriberOne);
            }
            catch (Exception)
            {
            }

            reset.WaitOne(1000);

            Assert.IsFalse(onSubscriptionErrorCalled);
        }

        public class NoneConstraintEvent : Event
        {
        }
    }
}
