using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class EventProxy<TProxyEvent>
    {
        private readonly IEventAggregator<TProxyEvent> eventAggregator;
        private bool queueSubscriptions = true;
        private readonly List<EventSubscriptionQueueItem> subscriptionQueue;
        private readonly IHubProxy proxy;

        public EventProxy(IEventAggregator<TProxyEvent> eventAggregator, string hubUrl)
        {
            subscriptionQueue = new List<EventSubscriptionQueueItem>();
            this.eventAggregator = eventAggregator;
            var connection = new HubConnection(hubUrl);
            proxy = connection.CreateHubProxy("EventAggregatorProxyHub");

            connection.Start().ContinueWith(o =>
                {
                    queueSubscriptions = false;
                    subscriptionQueue.ForEach(s => this.Subscribe(s.EventType, s.Constraint));
                    subscriptionQueue.Clear();

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
            var eventType = typeof(TProxyEvent).Assembly.GetType(data.type.Value);
            var e = data.@event as JObject;
            var @event = e.ToObject(eventType);

            eventAggregator.Publish(@event);
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
    }
}
