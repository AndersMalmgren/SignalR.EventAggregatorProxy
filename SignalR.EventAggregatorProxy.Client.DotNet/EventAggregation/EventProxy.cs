using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using SignalR.EventAggregatorProxy.Client.Event;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class EventProxy<TProxyEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private bool queueSubscriptions = true;
        private readonly List<EventSubscriptionQueueItem> subscriptionQueue;
        private readonly IHubProxy proxy;
        private readonly TypeFinder<TProxyEvent> typeFinder;

        public EventProxy(IEventAggregator eventAggregator, string hubUrl, Action<HubConnection> configureConnection = null)
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

        public void Subscribe(Type eventType, object constraint)
        {
            if (queueSubscriptions)
            {
                subscriptionQueue.Add(new EventSubscriptionQueueItem(eventType, constraint));
                return;
            }

            var typeName = GetNameWihoutGenerics(eventType);
            var genericArguments = eventType.GetGenericArguments().Select(ga => ga.FullName);
            var args = new List<object> { typeName, genericArguments };
            if(constraint != null)
                args.Add(constraint);
            
            proxy.Invoke("subscribe", args.ToArray());
        }

        private void OnEvent(dynamic data)
        {
            var @event = ParseTypeData(data);
            eventAggregator.Publish(@event);
        }

        private object ParseTypeData(dynamic data)
        {
            var jObject = data as JObject;
            var message = jObject.ToObject<Message>();

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

        public void Unsubscribe(IEnumerable<Type> types)
        {
            var unsubssciptions = types.Select(t => new { type = GetNameWihoutGenerics(t), genericArguments = t.GetGenericArguments().Select(ga => ga.FullName) });
            queueSubscriptions = true;
            proxy.Invoke("unsubscribe", unsubssciptions)
                .ContinueWith(o => SendQueuedSubscriptions());
        }

        private void SendQueuedSubscriptions()
        {
            queueSubscriptions = false;
            subscriptionQueue.ForEach(s => this.Subscribe(s.EventType, s.Constraint));
            subscriptionQueue.Clear();
        }

        private class EventSubscriptionQueueItem
        {
            public EventSubscriptionQueueItem(Type eventType, object constraint)
            {
                EventType = eventType;
                Constraint = constraint;
            }

            public Type EventType { get; set; }
            public Object Constraint { get; set; }
        }

        private class Message
        {
            public string Type { get; set; }
            public object Event { get; set; }
            public string[] GenericArguments { get; set; }
        }
    }
}
