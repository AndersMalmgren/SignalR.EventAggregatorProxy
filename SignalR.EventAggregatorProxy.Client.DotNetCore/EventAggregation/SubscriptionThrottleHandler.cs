using System;
using System.Threading.Tasks;
using System.Timers;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public class SubscriptionThrottleHandler : ISubscriptionThrottleHandler
    {
        private readonly Timer throttleTimer;

        public SubscriptionThrottleHandler()
        {
            throttleTimer = new Timer(32) { AutoReset = false };
        }

        public void Init(Func<Task> onThrottled)
        {
            throttleTimer.Elapsed += (s, e) => onThrottled().Wait();
        }

        public void Throttle()
        {
            throttleTimer.Stop();
            throttleTimer.Start();
        }
    }
}
