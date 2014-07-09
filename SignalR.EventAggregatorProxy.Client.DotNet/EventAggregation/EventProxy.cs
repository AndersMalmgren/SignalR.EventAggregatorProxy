using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using SignalR.EventAggregatorProxy.Client.Bootstrap;
using SignalR.EventAggregatorProxy.Client.Bootstrap.Factories;
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
        private readonly Timer throttleTimer;

        public EventProxy(IEventAggregator<TProxyEvent> eventAggregator, string hubUrl,
                          Action<IHubConnection> configureConnection = null)
        {
            typeFinder = new TypeFinder<TProxyEvent>();
            subscriptionQueue = new List<Subscription>();
            throttleTimer = new Timer(32);
            throttleTimer.AutoReset = false;
            throttleTimer.Elapsed += (s, e) => SendQueuedSubscriptions();

            this.eventAggregator = eventAggregator;
            proxy = DependencyResolver.Global.Get<IHubProxyFactory>()
                .Create(hubUrl, configureConnection, p =>
                {
                    SendQueuedSubscriptions();
                    p.On<Message>("onEvent", OnEvent);
                });
        }

        public void Subscribe(IEnumerable<Subscription> subscriptions)
        {
            subscriptionQueue.AddRange(subscriptions);
            if (!queueSubscriptions)
            {
                throttleTimer.Stop();
                throttleTimer.Start();
            }
        }

        private void OnEvent(Message message)
        {   
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
            
            return message.Event.ToObject(type);
        }
        
        public void Unsubscribe(IEnumerable<Subscription> subscriptions)
        {
            var unsubssciptions = subscriptions.Select(s => new { type = s.EventType.GetFullNameWihoutGenerics(), genericArguments = s.EventType.GetGenericArguments().Select(ga => ga.FullName), id = s.ConstraintId });
            if (unsubssciptions.Any())
            {
                queueSubscriptions = true;
                proxy.Invoke("unsubscribe", unsubssciptions)
                     .ContinueWith(o => SendQueuedSubscriptions());
            }
        }

        private void SendQueuedSubscriptions()
        {
            queueSubscriptions = false;
            var subscriptions = subscriptionQueue.Select(s => new
                {
                    Type = s.EventType.GetFullNameWihoutGenerics(),
                    GenericArguments = s.EventType.GetGenericArguments().Select(ga => ga.FullName),
                    s.Constraint,
                    s.ConstraintId
                }).ToList();
            subscriptionQueue.Clear();

            if (subscriptions.Any())
                proxy.Invoke("subscribe", subscriptions);
        }

        private class Message
        {
            public string Type { get; set; }
            public JObject Event { get; set; }
            public string[] GenericArguments { get; set; }
            public int? Id { get; set; }
        }
    }
}
