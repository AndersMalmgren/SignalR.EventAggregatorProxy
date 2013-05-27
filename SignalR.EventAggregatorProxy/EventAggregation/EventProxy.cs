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
        private readonly IDictionary<string, List<Type>> userSubscriptions;

        public EventProxy()
        {
            this.typeFinder = GlobalHost.DependencyResolver.Resolve<ITypeFinder>();
            var eventAggregator = GlobalHost.DependencyResolver.Resolve<IEventAggregator>();
            subscriptions = typeFinder
                .ListEventTypes()
                .ToDictionary(t => t.GUID, t => new List<Subscription>());

            userSubscriptions = new Dictionary<string, List<Type>>();

            eventAggregator.Subscribe(Handle);
        }

        public void Subscribe(HubCallerContext context, string typeName, IEnumerable<string> genericArguments, dynamic constraint)
        {
            var type = typeFinder.GetEventType(typeName);
            var genericArgumentTypes = genericArguments.Select(typeFinder.GetType).ToList();
            subscriptions[type.GUID].Add(new Subscription(context.ConnectionId, context.User.Identity.Name, constraint, genericArgumentTypes));
            if (!userSubscriptions.ContainsKey(context.ConnectionId))
            {
                userSubscriptions[context.ConnectionId] = new List<Type>();
            }
            userSubscriptions[context.ConnectionId].Add(type);
        }

        public void UnsubscribeConnection(string connectionId)
        {
            if (userSubscriptions.ContainsKey(connectionId))
            {
                foreach (var type in userSubscriptions[connectionId])
                {
                    var contexts = subscriptions[type.GUID];
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
                    userSubscriptions[connectionId].Remove(type.Type);
                }
                subscriptions[type.Type.GUID].RemoveAll(s => s.ConnectionId == connectionId && GenericArgumentsCorrect(s, type.ClientData.genericArguments));
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
                client.onEvent(new Message(eventType.GetFullNameWihoutGenerics(), message, genericArguments));
            }
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
            public Message(string type, object @event, string[] genericArguments)
            {
                Type = type;
                Event = @event;
                GenericArguments = genericArguments;
            }

            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("event")]
            public object Event { get; set; }
            [JsonProperty("genericArguments")]
            public string[] GenericArguments  { get; set; }
        }
    }
}
