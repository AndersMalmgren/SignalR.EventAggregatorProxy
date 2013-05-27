using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.Hubs
{
    public class EventAggregatorProxyHub : Hub
    {
        private static readonly EventProxy eventProxy;

        static EventAggregatorProxyHub()
        {
            eventProxy = new EventProxy();
        }

        public void Subscribe(string type, string[] genericTypes, dynamic contraint)
        {
            eventProxy.Subscribe(Context, type, genericTypes ?? new string[0], contraint);
        }

        public void Unsubscribe(IEnumerable<EventType> types)
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
