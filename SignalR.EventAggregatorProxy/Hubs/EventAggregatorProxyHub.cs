using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Hubs
{
    public class EventAggregatorProxyHub : Hub
    {
        private static readonly EventProxy eventProxy;

        static EventAggregatorProxyHub()
        {
            eventProxy = new EventProxy();
        }

        public void Subscribe(string type, dynamic contraint)
        {
            eventProxy.Subscribe(Context, type, contraint);
        }

        public void Unsubscribe(IEnumerable<string> types)
        {
            eventProxy.Unsubscribe(Context.ConnectionId, types);
        }

        public override Task OnDisconnected()
        {
            eventProxy.UnsubscribeConnection(Context.ConnectionId);
            return base.OnDisconnected();
        }
    }
}
