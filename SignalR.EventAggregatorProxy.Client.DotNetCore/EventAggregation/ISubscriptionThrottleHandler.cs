using System;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public interface ISubscriptionThrottleHandler
    {
        void Throttle();
        void Init(Func<Task> onThrottled);
    }
}