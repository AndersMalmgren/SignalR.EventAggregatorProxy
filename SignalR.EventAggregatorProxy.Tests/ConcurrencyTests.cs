using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.Tests
{
    [TestClass]
    public class When_concurrent_operations_are_performed_on_proxy_event : Test
    {
        private bool running = true;
        private AutoResetEvent reset;
        private bool failed = false;
        private ConcurrentBag<string> ids;
            
        [TestInitialize]
        public void Context()
        {
            ids = new ConcurrentBag<string>();
            reset = new AutoResetEvent(false);
            var running = true;
            var count = 0;

            var type = typeof (MembersEvent);
            var typeName = type.FullName;
            var typeNames = new[] {typeName}.Select(t => new EventType { Type = t }).ToList();

            Action<object> handler = null;
            WhenCalling<ITypeFinder>(x => x.ListEventTypes()).Return(new[] {type});
            WhenCalling<ITypeFinder>(x => x.GetEventType(Arg<string>.Is.Anything)).Return(type);
            WhenCalling<IEventAggregator>(x => x.Subscribe(Arg<Action<object>>.Is.Anything)).Callback<Action<object>>(h =>
            {
                handler = h;
                return true;
            });
            WhenAccessing<IRequest, IPrincipal>(x => x.User).Return(Thread.CurrentPrincipal);

            var client = new Client(() => { 
                count++;
                if (count == 5000)
                    reset.Set();

            });

            WhenCalling<IHubConnectionContext>(x => x.Client(Arg<string>.Is.Anything)).Return(client);
            WhenAccessing<IHubContext, IHubConnectionContext>(x => x.Clients).Return(Get<IHubConnectionContext>());
            Mock<IConnectionManager>();
            WhenCalling<IConnectionManager>(x => x.GetHubContext<EventAggregatorProxyHub>()).Return(Get<IHubContext>());

            var eventProxy = new EventProxy();


            FailIfThreadCrashes(() => eventProxy.Subscribe(CreateHubContext(), typeName, new string[0], null, null));
            FailIfThreadCrashes(() =>
                {
                    string id;
                    if (ids.TryTake(out id))
                    {
                        eventProxy.Unsubscribe(id, typeNames);
                    }
                    
                });
            FailIfThreadCrashes(() =>
                {
                    string id;
                    if (ids.TryTake(out id))
                    {
                        eventProxy.UnsubscribeConnection(id);
                    }
                });

            FailIfThreadCrashes(() => handler(new MembersEvent()));

            reset.WaitOne();
            running = false;
        }
        
        private HubCallerContext CreateHubContext()
        {
            var id = Guid.NewGuid().ToString();
            ids.Add(id);

            return new HubCallerContext(Get<IRequest>(), id);
        }

        [TestMethod]
        public void It_should_work_with_concurent_operations()
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
                        reset.Set();
                        Assert.Fail("Not thread safe: {0}", e.Message);
                    }
                }
            });
        }

        private void FailIfThreadCrashes<T>(Func<T> func)
        {
            FailIfThreadCrashes(() => { var x = func(); });
        }

        public class Client
        {
            private readonly Action callback;

            public Client(Action callback)
            {
                this.callback = callback;
            }

            public void onEvent(object message)
            {
                callback();
            }
        }
    }
}
