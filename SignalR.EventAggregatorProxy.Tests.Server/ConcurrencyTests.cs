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

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    [TestClass]
    public class When_concurrent_operations_are_performed_on_proxy_event : EventProxyTest
    {
        private bool running = true;
        private AutoResetEvent reset;
        private bool failed = false;
        private TimeSpan benchmarkTime = TimeSpan.FromSeconds(5);
            
        [TestInitialize]
        public void Context()
        {
            reset = new AutoResetEvent(false);
            var handler = SetupProxy(typeof(MembersEvent));

            FailIfThreadCrashes(Subscribe);
            FailIfThreadCrashes(() =>
                {
                    if (ids.Count < 100) return;

                    string id;
                    if (ids.TryTake(out id))
                    {
                        Unsubscribe(id);
                    }

                });
            FailIfThreadCrashes(() =>
                {
                    if (ids.Count < 100) return;

                    string id;
                    if (ids.TryTake(out id))
                    {
                        UnsubscribeConnection(id);
                    }
                });

            FailIfThreadCrashes(() => handler(new MembersEvent()));
            FailIfThreadCrashes(() => handler(new MembersEvent()));
            FailIfThreadCrashes(() => handler(new MembersEvent()));
            FailIfThreadCrashes(() => handler(new MembersEvent()));

            var timer = new System.Timers.Timer(benchmarkTime.TotalMilliseconds);
            timer.Elapsed += (s, e) => reset.Set();
            var start = DateTime.Now;
            timer.Start();

            reset.WaitOne();
            running = false;
            Console.WriteLine("Catched events: {0} in {1}", events.Count, DateTime.Now - start);
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
    }
}
