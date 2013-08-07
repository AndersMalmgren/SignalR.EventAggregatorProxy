using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Event;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class EventProxy<TProxyEvent>
    {
        private readonly IEventAggregator<TProxyEvent> eventAggregator;
        private bool queueSubscriptions = true;
        private readonly List<EventSubscriptionQueueItem> subscriptionQueue;
        private readonly IHubProxy proxy;
        private readonly TypeFinder<TProxyEvent> typeFinder;

        public EventProxy(IEventAggregator<TProxyEvent> eventAggregator, string hubUrl, Action<HubConnection> configureConnection = null)
        {
            typeFinder = new TypeFinder<TProxyEvent>();
            subscriptionQueue = new List<EventSubscriptionQueueItem>();
            this.eventAggregator = eventAggregator;
            var connection = new HubConnection(hubUrl);
            if (configureConnection != null)
                configureConnection(connection);

            proxy = connection.CreateHubProxy("EventAggregatorProxyHub");
            
            connection.Start().ContinueWith(o =>
                {
                    SendQueuedSubscriptions();

                    proxy.On<object>("onEvent", OnEvent);
                });
        }

        public void Subscribe(Type eventType, object constraint, int? constraintId)
        {
            if (queueSubscriptions)
            {
                subscriptionQueue.Add(new EventSubscriptionQueueItem(eventType, constraint, constraintId));
                return;
            }

            var typeName = GetNameWihoutGenerics(eventType);
            var genericArguments = eventType.GetGenericArguments().Select(ga => ga.FullName);
            var args = new List<object> { typeName, genericArguments };
            if (constraint != null)
                args.AddRange(new[] { constraint, constraintId });

            proxy.Invoke("subscribe", args.ToArray());
        }

        private void OnEvent(dynamic data)
        {   
            var jObject = data as JObject;
            var message = jObject.ToObject<Message>();

            System.Diagnostics.Debug.WriteLine(message.Type);

            var @event = ParseTypeData(message);
            eventAggregator.Publish(@event, message.Id);
        }

        private dynamic ParseTypeData(Message message)
        {
            Type type = typeFinder.GetEventType(message.Type);
            if (message.GenericArguments.Length > 0)
            {
                var genericArguments = message.GenericArguments
                    .Select(typeFinder.GetType)
                    .ToArray();

                type = type.MakeGenericType(genericArguments);
            }
            
            return (message.Event as JObject).ToObject(type);
        }

        private string GetNameWihoutGenerics(Type type)
        {
            var name = type.FullName;
            if (type.IsGenericType)
            {
                return name.Substring(0, name.IndexOf("`", StringComparison.Ordinal));
            }

            return name;
        }
        

        public void Unsubscribe(IEnumerable<Type> types, IEnumerable<IConstraintInfo> constraintInfos)
        {
            var unsubssciptions = types.Select(t => new { type = GetNameWihoutGenerics(t), genericArguments = t.GetGenericArguments().Select(ga => ga.FullName), id = constraintInfos.GetConstraintId(t) });
            queueSubscriptions = true;
            proxy.Invoke("unsubscribe", unsubssciptions)
                .ContinueWith(o => SendQueuedSubscriptions());
        }

        private void SendQueuedSubscriptions()
        {
            queueSubscriptions = false;
            subscriptionQueue.ForEach(s => this.Subscribe(s.EventType, s.Constraint, s.ConstraintId));
            subscriptionQueue.Clear();
        }

        private class EventSubscriptionQueueItem
        {
            public EventSubscriptionQueueItem(Type eventType, object constraint, int? constraintId)
            {
                EventType = eventType;
                Constraint = constraint;
                ConstraintId = constraintId;
            }

            public Type EventType { get; set; }
            public Object Constraint { get; set; }
            public int? ConstraintId { get; set; }
        }

        private class Message
        {
            public string Type { get; set; }
            public object Event { get; set; }
            public string[] GenericArguments { get; set; }
            public int? Id { get; set; }
        }
    }
}
