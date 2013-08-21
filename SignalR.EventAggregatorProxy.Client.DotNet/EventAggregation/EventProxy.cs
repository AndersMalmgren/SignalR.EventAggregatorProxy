using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Event;
using SignalR.EventAggregatorProxy.Client.Extensions;
using Subscription = SignalR.EventAggregatorProxy.Client.Model.Subscription;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class EventProxy<TProxyEvent>
    {
        private readonly IEventAggregator<TProxyEvent> eventAggregator;
        private bool queueSubscriptions = true;
        private readonly List<Subscription> subscriptionQueue;
        private readonly IHubProxy proxy;
        private readonly TypeFinder<TProxyEvent> typeFinder;

        public EventProxy(IEventAggregator<TProxyEvent> eventAggregator, string hubUrl, Action<HubConnection> configureConnection = null)
        {
            typeFinder = new TypeFinder<TProxyEvent>();
            subscriptionQueue = new List<Subscription>();
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

        public void Subscribe(IEnumerable<Subscription> subscriptions)
        {
            if (queueSubscriptions)
            {
                subscriptionQueue.AddRange(subscriptions);
                return;
            }

            proxy.Invoke("subscribe", subscriptions.Select(s => new
            {
                Type = s.EventType.GetFullNameWihoutGenerics(),
                GenericArguments = s.EventType.GetGenericArguments().Select(ga => ga.FullName),
                s.Constraint,
                s.ConstraintId
            }));
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
        
        public void Unsubscribe(IEnumerable<Type> types, IEnumerable<IConstraintInfo> constraintInfos)
        {
            var unsubssciptions = types.Select(t => new { type = t.GetFullNameWihoutGenerics(), genericArguments = t.GetGenericArguments().Select(ga => ga.FullName), id = constraintInfos.GetConstraintId(t) });
            queueSubscriptions = true;
            proxy.Invoke("unsubscribe", unsubssciptions)
                .ContinueWith(o => SendQueuedSubscriptions());
        }

        private void SendQueuedSubscriptions()
        {
            queueSubscriptions = false;
            Subscribe(subscriptionQueue);
            subscriptionQueue.Clear();
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
