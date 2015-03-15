using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Newtonsoft.Json;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Extensions;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public class EventProxy<TEvent> : EventProxy, IEventAggregatorProxy<TEvent>
    {
        public void Publish(TEvent message)
        {
            var eventType = message.GetType();
            var genericArguments = eventType.GetGenericArguments().Select(t => t.FullName).ToArray();

            var context = GlobalHost.DependencyResolver.Resolve<IConnectionManager>().GetHubContext<EventAggregatorProxyHub>();
            var constraintHandlerType = typeFinder.GetConstraintHandlerType(eventType);
            var constraintHandler = (constraintHandlerType != null ? GlobalHost.DependencyResolver.GetService(constraintHandlerType) : null) as IEventConstraintHandler;

            if (constraintHandlerType != null && constraintHandler == null)
                throw new Exception(string.Format("Constraint {0} not registered correctly with the DependencyResolver", constraintHandlerType.Name));

            foreach (var subscription in subscriptions[eventType.GUID])
            {
                if (!GenericArgumentsCorrect(eventType, subscription)) continue;

                if (constraintHandler != null && !constraintHandler.Allow(message, subscription.Username, subscription.Constraint))
                    continue;

                var client = context.Clients.Client(subscription.ConnectionId);
                client.onEvent(new Message(eventType.GetFullNameWihoutGenerics(), message, genericArguments, subscription.ConstraintId));
            }
        }

        private bool GenericArgumentsCorrect(Type eventType, Subscription subscription)
        {
            if (!eventType.IsGenericType) return true;

            var genericArguments = eventType.GetGenericArguments();
            if (genericArguments.Length != subscription.GenericArguments.Count) return false;

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
            public string[] GenericArguments { get; set; }
            [JsonProperty("id")]
            public int? ConstraintId { get; set; }

        }
    }

    public abstract class EventProxy
    {
        protected readonly ITypeFinder typeFinder;
        protected readonly IDictionary<Guid, List<Subscription>> subscriptions;
        protected readonly IDictionary<string, List<Subscription>> userSubscriptions;

        protected EventProxy()
        {
            this.typeFinder = GlobalHost.DependencyResolver.Resolve<ITypeFinder>();
            //var eventAggregator = GlobalHost.DependencyResolver.Resolve<IEventAggregatorProxy>();
            subscriptions = new Dictionary<Guid, List<Subscription>>(typeFinder
                .ListEventTypes()
                .ToDictionary(t => t.GUID, t => new List<Subscription>()));

            userSubscriptions = new Dictionary<string, List<Subscription>>();

            //eventAggregator.Subscribe(Handle);
        }

        public void Subscribe(HubCallerContext context, string typeName, IEnumerable<string> genericArguments, dynamic constraint, int? constraintId)
        {
            lock (this)
            {
                var type = typeFinder.GetEventType(typeName);
                var genericArgumentTypes = genericArguments.Select(typeFinder.GetType).ToList();
                var subscription = new Subscription(type, context.ConnectionId, context.User.Identity.Name, constraint,
                                                    constraintId, genericArgumentTypes);
                subscriptions[type.GUID] = new List<Subscription>(subscriptions[type.GUID]) { subscription };

                if (!userSubscriptions.ContainsKey(context.ConnectionId))
                {
                    userSubscriptions[context.ConnectionId] = new List<Subscription>();
                }
                userSubscriptions[context.ConnectionId] = new List<Subscription>(userSubscriptions[context.ConnectionId]) { subscription };
            }
        }

        public void UnsubscribeConnection(string connectionId)
        {
            lock (this)
            {
                if (userSubscriptions.ContainsKey(connectionId))
                {
                    foreach (var subscription in userSubscriptions[connectionId])
                    {
                        subscriptions[subscription.EventType.GUID] =
                            new List<Subscription>(subscriptions[subscription.EventType.GUID].Where(c => c.ConnectionId != connectionId));

                    }
                    userSubscriptions.Remove(connectionId);
                }
            }
        }

        public void Unsubscribe(string connectionId, IEnumerable<EventType> typeNames)
        {
            lock (this)
            {
                foreach (var type in typeNames.Select(t => new { Type = typeFinder.GetEventType(t.Type), ClientData = t }))
                {
                    if (userSubscriptions.ContainsKey(connectionId))
                    {
                        userSubscriptions[connectionId] =
                            new List<Subscription>(userSubscriptions[connectionId].Where(s => !(s.EventType.GUID == type.Type.GUID && GenericArgumentsCorrect(s, type.ClientData.GenericArguments) && ConstraintIdCorrect(s, type.ClientData.ConstraintId))));
                    }
                    subscriptions[type.Type.GUID] =
                        new List<Subscription>(subscriptions[type.Type.GUID].Where(s => !(s.ConnectionId == connectionId && GenericArgumentsCorrect(s, type.ClientData.GenericArguments) && GenericArgumentsCorrect(s, type.ClientData.GenericArguments) && ConstraintIdCorrect(s, type.ClientData.ConstraintId))));
                }
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
    }
}
