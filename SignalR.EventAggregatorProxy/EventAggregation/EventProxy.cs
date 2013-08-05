using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Extensions;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public class EventProxy
    {
        private readonly ITypeFinder typeFinder;
        private readonly IDictionary<Guid, List<Subscription>> subscriptions;
        private readonly IDictionary<string, List<Subscription>> userSubscriptions;

        public EventProxy()
        {
            this.typeFinder = GlobalHost.DependencyResolver.Resolve<ITypeFinder>();
            var eventAggregator = GlobalHost.DependencyResolver.Resolve<IEventAggregator>();
            subscriptions = typeFinder
                .ListEventTypes()
                .ToDictionary(t => t.GUID, t => new List<Subscription>());

            userSubscriptions = new Dictionary<string, List<Subscription>>();

            eventAggregator.Subscribe(Handle);
        }

        public void Subscribe(HubCallerContext context, string typeName, IEnumerable<string> genericArguments, dynamic constraint, int? constraintId)
        {
            var type = typeFinder.GetEventType(typeName);
            var genericArgumentTypes = genericArguments.Select(typeFinder.GetType).ToList();
            var subscription = new Subscription(type, context.ConnectionId, context.User.Identity.Name, constraint, constraintId, genericArgumentTypes);
            subscriptions[type.GUID].Add(subscription);
            if (!userSubscriptions.ContainsKey(context.ConnectionId))
            {
                userSubscriptions[context.ConnectionId] = new List<Subscription>();
            }
            userSubscriptions[context.ConnectionId].Add(subscription);
        }

        public void UnsubscribeConnection(string connectionId)
        {
            if (userSubscriptions.ContainsKey(connectionId))
            {
                foreach (var subscription in userSubscriptions[connectionId])
                {
                    var contexts = subscriptions[subscription.EventType.GUID];
                    contexts.RemoveAll(c => c.ConnectionId == connectionId);
                }
                userSubscriptions.Remove(connectionId);
            }
        }

        public void Unsubscribe(string connectionId, IEnumerable<EventType> typeNames)
        {
            foreach (var type in typeNames.Select(t => new { Type = typeFinder.GetEventType(t.Type), ClientData = t }))
            {
                if (userSubscriptions.ContainsKey(connectionId))
                {
                    userSubscriptions[connectionId].RemoveAll(s => s.EventType.GUID == type.Type.GUID && GenericArgumentsCorrect(s, type.ClientData.GenericArguments) && ConstraintIdCorrect(s, type.ClientData.ConstraintId));
                }
                subscriptions[type.Type.GUID].RemoveAll(s => s.ConnectionId == connectionId && GenericArgumentsCorrect(s, type.ClientData.GenericArguments) && GenericArgumentsCorrect(s, type.ClientData.GenericArguments) && ConstraintIdCorrect(s, type.ClientData.ConstraintId));
            }
        }

        private void Handle(object message)
        {
            var eventType = message.GetType();
            var genericArguments = eventType.GetGenericArguments().Select(t => t.FullName).ToArray();

            var context = GlobalHost.ConnectionManager.GetHubContext<EventAggregatorProxyHub>();
            var constraintHandlerType = typeFinder.GetConstraintHandlerType(eventType);
            var constraintHandler = (constraintHandlerType != null ? GlobalHost.DependencyResolver.GetService(constraintHandlerType) : null) as IEventConstraintHandler;
            foreach (var subscription in subscriptions[eventType.GUID])
            {
                if(!GenericArgumentsCorrect(eventType, subscription)) continue;

                if (constraintHandler != null && !constraintHandler.Allow(message, subscription.Username, subscription.Constraint))
                    continue;

                var client = context.Clients.Client(subscription.ConnectionId);
                client.onEvent(new Message(eventType.GetFullNameWihoutGenerics(), message, genericArguments, subscription.ConstraintId));
            }
        }

        private bool ConstraintIdCorrect(Subscription subscription, int? constraintId)
        {
            return subscription.ConstraintId == null || subscription.ConstraintId == constraintId;
        }

        private bool GenericArgumentsCorrect(Subscription subscription, string[] genericArguments)
        {
            if (genericArguments == null) return true;

            if (genericArguments.Length != subscription.GenericArguments.Count) return false;
            var correctArguments = genericArguments.Where((t, i) => subscription.GenericArguments[i].FullName == t);
            return correctArguments.Count() == genericArguments.Length;
        }

        private bool GenericArgumentsCorrect(Type eventType, Subscription subscription)
        {
            if (!eventType.IsGenericType) return true;

            var genericArguments = eventType.GetGenericArguments();
            if(genericArguments.Length != subscription.GenericArguments.Count) return false;

            var correctArguments = genericArguments.Where((t, i) => subscription.GenericArguments[i] == t);
            return correctArguments.Count() == genericArguments.Length;
        }

        private class Message
        {
            public Message(string type, object @event, string[] genericArguments, int? constraintId)
            {
                Type = type;
                Event = @event;
                GenericArguments = genericArguments;
                ConstraintId = constraintId;
            }

            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("event")]
            public object Event { get; set; }
            [JsonProperty("genericArguments")]
            public string[] GenericArguments  { get; set; }
            [JsonProperty("id")]
            public int? ConstraintId { get; set; }

        }
    }
}
