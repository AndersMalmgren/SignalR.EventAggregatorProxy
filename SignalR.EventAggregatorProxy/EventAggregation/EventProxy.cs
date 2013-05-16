using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Hubs;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public class EventProxy
    {
        private readonly ITypeFinder typeFinder;
        private readonly IDictionary<Type, List<Subscription>> subscriptions;
        private readonly IDictionary<string, List<Type>> userSubscriptions;

        public EventProxy()
        {
            this.typeFinder = GlobalHost.DependencyResolver.Resolve<ITypeFinder>();
            var eventAggregator = GlobalHost.DependencyResolver.Resolve<IEventAggregator>();
            subscriptions = typeFinder
                .ListEventTypes()
                .ToDictionary(t => t, t => new List<Subscription>());

            userSubscriptions = new Dictionary<string, List<Type>>();

            eventAggregator.Subscribe(Handle);
        }

        public void Subscribe(string connectionId, string typeName, dynamic constraint)
        {
            var type = typeFinder.GetType(typeName);
            subscriptions[type].Add(new Subscription(connectionId, type, constraint));
            if (!userSubscriptions.ContainsKey(connectionId))
            {
                userSubscriptions[connectionId] = new List<Type>();
            }
            userSubscriptions[connectionId].Add(type);
        }

        public void UnsubscribeConnection(string connectionId)
        {
            if (userSubscriptions.ContainsKey(connectionId))
            {
                foreach (var type in userSubscriptions[connectionId])
                {
                    var contexts = subscriptions[type];
                    contexts.RemoveAll(c => c.ConnectionId == connectionId);
                }
                userSubscriptions.Remove(connectionId);
            }
        }

        public void Unsubscribe(string connectionId, IEnumerable<string> typeNames)
        {
            foreach (var type in typeNames.Select(typeFinder.GetType))
            {
                if (userSubscriptions.ContainsKey(connectionId))
                {
                    userSubscriptions[connectionId].Remove(type);
                }
                subscriptions[type].RemoveAll(s => s.ConnectionId == connectionId);
            }
        }

        private void Handle(object message)
        {
            var eventType = message.GetType();

            var context = GlobalHost.ConnectionManager.GetHubContext<EventAggregatorProxyHub>();
            var constraintHandlerType = typeFinder.GetConstraintHandlerType(eventType);
            var constraintHandler = (constraintHandlerType != null ? GlobalHost.DependencyResolver.GetService(constraintHandlerType) : null) as IEventConstraintHandler;
            foreach (var subscription in subscriptions[eventType])
            {
                if (constraintHandler != null && !constraintHandler.Allow(message, null, subscription.Constraint))
                    continue;

                var client = context.Clients.Client(subscription.ConnectionId);
                client.onEvent(new Message(eventType.FullName, message));
            }
        }

        private class Message
        {
            public Message(string type, object @event)
            {
                Type = type;
                Event = @event;
            }

            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("event")]
            public object Event { get; set; }
        }
    }
}
