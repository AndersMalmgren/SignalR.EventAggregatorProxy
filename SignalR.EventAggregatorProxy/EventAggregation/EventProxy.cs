using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly IHubContext<EventAggregatorProxyHub> hubContext;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<EventProxy> logger;

        public EventProxy(ITypeFinder typefinder, IEventAggregator eventAggregator, IHubContext<EventAggregatorProxyHub> hubContext, IServiceProvider serviceProvider, ILogger<EventProxy> logger)
        {
            this.typeFinder = typefinder;
            this.hubContext = hubContext;
            this.serviceProvider = serviceProvider;
            this.logger = logger;

            subscriptions = new Dictionary<Guid, List<Subscription>>(typeFinder
                .ListEventTypes()
                .ToDictionary(t => t.GUID, t => new List<Subscription>()));

            userSubscriptions = new Dictionary<string, List<Subscription>>();

            eventAggregator.Subscribe(Handle);
        }

        public void Subscribe(HubCallerContext context, string typeName, IEnumerable<string> genericArguments, JsonElement constraint, int? constraintId)
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

        private async Task Handle(object message)
        {
            var eventType = message.GetType();
            var genericArguments = eventType.GetGenericArguments().Select(t => t.FullName).ToArray();
            
            var constraintHandlerTypes = typeFinder.GetConstraintHandlerTypes(eventType);
            var hasHandlerTypes = constraintHandlerTypes.Any();
            var constraintHandlers = (hasHandlerTypes ? constraintHandlerTypes.Select(t => serviceProvider.GetService(t) as IEventConstraintHandler).ToList() : Enumerable.Empty<IEventConstraintHandler>());

            if (hasHandlerTypes && constraintHandlers.Any(h => h == null))
                throw new Exception(string.Format("Constraint(s) {0} not registered correctly with the DependencyResolver", string.Join("; ",constraintHandlerTypes.Select(t=> t.Name))));

            foreach (var subscription in subscriptions[eventType.GUID])
            {
                try
                {
                    if (!GenericArgumentsCorrect(eventType, subscription)) continue;

                    if (hasHandlerTypes && constraintHandlers.Any(handler => !handler.Allow(message, new ConstraintContext(subscription.ConnectionId, subscription.Username), subscription.Constraint)))
                        continue;

                    var client = hubContext.Clients.Client(subscription.ConnectionId);
                    await client.SendCoreAsync("onEvent", new object[] { new Message(eventType.GetFullNameWihoutGenerics(), message, genericArguments, subscription.ConstraintId) } );
                }
                catch(Exception e)
                {
                    logger.LogWarning(e, "Error while publishing event to subscribers");
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

            public string Type { get; set; }
            public object Event { get; set; }
            public string[] GenericArguments { get; set; }
            public int? ConstraintId { get; set; }

        }
    }
}
