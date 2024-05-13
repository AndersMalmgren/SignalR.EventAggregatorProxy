using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Extensions;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.Hubs
{
    public class EventAggregatorProxyHub(EventProxy eventProxy) : Hub
    {
        public void Subscribe(IEnumerable<SubscriptionDto> subscriptions, bool reconnected)
        {
            if (reconnected)
                eventProxy.UnsubscribeConnection(Context.ConnectionId);

            subscriptions
                .ForEach(s => eventProxy.Subscribe(Context, s.Type, s.GenericArguments ?? Array.Empty<string>(), s.Constraint, s.ConstraintId));
        }

        public void Unsubscribe(IEnumerable<EventType> types)
        {
            eventProxy.Unsubscribe(Context.ConnectionId, types);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            eventProxy.UnsubscribeConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
