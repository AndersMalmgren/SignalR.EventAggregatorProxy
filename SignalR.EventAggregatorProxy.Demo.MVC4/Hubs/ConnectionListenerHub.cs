using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.Hubs
{
    public class ConnectionListenerHub : Hub
    {
        private readonly IEventAggregator eventAggregator;

        public ConnectionListenerHub(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public override Task OnConnected()
        {
            eventAggregator.Publish(new ConnectionStateChangedEvent(Context.ConnectionId, true));

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            eventAggregator.Publish(new ConnectionStateChangedEvent(Context.ConnectionId, false));

            return base.OnDisconnected(stopCalled);
        }
    }
}