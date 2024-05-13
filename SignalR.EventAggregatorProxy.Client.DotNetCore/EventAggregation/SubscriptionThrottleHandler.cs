using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public class SubscriptionThrottleHandler : ISubscriptionThrottleHandler
    {
        private Func<Task> onThrottled = null!;
        private CancellationTokenSource cancelSource = new();

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
