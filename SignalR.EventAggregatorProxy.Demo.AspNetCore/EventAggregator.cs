using System;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore
{
    public interface IEventAggregator : EventAggregation.IEventAggregator
    {
        Task Publish<T>(T message);
    }

    public class EventAggregator : IEventAggregator
    {
        private Func<object, Task> handler;

        public void Subscribe(Func<object, Task> handler)
        {
            this.handler = handler;
        }

        public async Task Publish<T>(T message)
        {
            if (handler != null)
                await handler(message);
        }
    }
}
