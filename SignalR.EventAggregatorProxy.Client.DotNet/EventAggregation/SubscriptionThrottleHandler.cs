using System;
using System.Timers;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class SubscriptionThrottleHandler : ISubscriptionThrottleHandler
    {
        private readonly Timer throttleTimer;

        public SubscriptionThrottleHandler()
        {
            throttleTimer = new Timer(32) { AutoReset = false };
        }

        public void Init(Action onThrottled)
        {
            throttleTimer.Elapsed += (s, e) => onThrottled();
        }

        public void Throttle()
        {
            throttleTimer.Stop();
            throttleTimer.Start();
        }
    }
}
