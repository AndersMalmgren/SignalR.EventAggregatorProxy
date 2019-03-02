using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    [TestClass]
    public class When_concurrent_operations_are_performed_on_proxy_event : EventProxyTest
    {
        private bool running = true;
        private AutoResetEvent reset;
        private bool failed = false;
        private TimeSpan benchmarkTime = TimeSpan.FromSeconds(5);

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            SetupProxy(serviceCollection, typeof(MembersEvent));
        }

        [TestInitialize]
        public void Context()
        {
            reset = new AutoResetEvent(false);
            var dummy = EventProxy; //Make sure handler is setup

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

            FailIfThreadCrashes(() => handler(new MembersEvent()).Wait());
            FailIfThreadCrashes(() => handler(new MembersEvent()).Wait());
            FailIfThreadCrashes(() => handler(new MembersEvent()).Wait());
            FailIfThreadCrashes(() => handler(new MembersEvent()).Wait());

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
