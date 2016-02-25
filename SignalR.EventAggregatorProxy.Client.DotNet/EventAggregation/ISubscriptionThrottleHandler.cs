using System;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public interface ISubscriptionThrottleHandler
    {
        void Throttle();
        void Init(Action onThrottled);
    }
}