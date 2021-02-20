using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public class SubscriptionThrottleHandler : ISubscriptionThrottleHandler
    {
        private Func<Task> onThrottled;
        private CancellationTokenSource cancelSource = new CancellationTokenSource();

        public void Init(Func<Task> onThrottled)
        {
            this.onThrottled = onThrottled;
        }

        public void Throttle()
        {
            cancelSource.Cancel();
            cancelSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                await Task.Delay(1);
                await onThrottled();

            }, cancelSource.Token);
        }
    }
}
