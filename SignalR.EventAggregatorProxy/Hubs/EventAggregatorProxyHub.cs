using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Extensions;
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

        public void Subscribe(IEnumerable<SubscriptionDto> subscriptions)
        {
            subscriptions
                .ForEach(s => eventProxy.Subscribe(Context, s.Type, s.GenericArguments ?? new string[0], s.Constraint, s.ConstraintId));
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
