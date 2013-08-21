using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Client.Hubs;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Extensions;
using Subscription = SignalR.EventAggregatorProxy.Client.Model.Subscription;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class EventAggregator : IEventAggregator
    {
        private readonly WeakReferenceList<object> subscribers = new WeakReferenceList<object>();

        public virtual void Subscribe(object subscriber)
        {
            subscribers.Add(subscriber);
        }

        public void Publish<T>(T message) where T : class
        {
            Publish(ListSubscribers(message), message);
        }

        protected virtual void Publish<T>(IEnumerable<IHandle<T>> filteredSubscribers,  T message) where T : class
        {
            filteredSubscribers
                .ForEach(s => s.Handle(message));
        }

        protected IEnumerable<IHandle<T>> ListSubscribers<T>(T message) where T : class
        {
            return subscribers
                .OfType<IHandle<T>>();
        } 

        public virtual void Unsubscribe(object subscriber)
        {
            subscribers.Remove(subscriber);
        }
    }

    public class EventAggregator<TProxyEvent> : EventAggregator, IEventAggregator<TProxyEvent>
    {
        private EventProxy<TProxyEvent> eventProxy;
        private readonly Dictionary<object, IEnumerable<IConstraintInfo>> constraints = new Dictionary<object, IEnumerable<IConstraintInfo>>();

        public EventAggregator<TProxyEvent> Init(string hubUrl, Action<HubConnection> configureConnection = null)
        {
            if (eventProxy != null) throw new Exception("Event aggregator already initialized");

            eventProxy = new EventProxy<TProxyEvent>(this, hubUrl, configureConnection);
            return this;
        }

        public override void Subscribe(object subscriber)
        {
            Subscribe(subscriber, new List<IConstraintInfo>());
        }

        public void Subscribe(object subscriber, IEnumerable<IConstraintInfo> constraintInfos)
        {
            base.Subscribe(subscriber);
            if (eventProxy != null)
            {
                constraints[subscriber] = constraintInfos;
                var proxyEvents = GetProxyEventTypes(subscriber);
                var subscriptions = proxyEvents
                    .Select(pe => new Subscription(pe, constraintInfos.GetConstraint(pe), constraintInfos.GetConstraintId(pe)))
                    .ToList();

                eventProxy.Subscribe(subscriptions);
            }
        }

        public void Publish<T>(T message, int? constraintId) where T : class
        {
            var subscribers = ListSubscribers(message)
                .Where(s => !constraintId.HasValue || constraints[s].Any(c => c.Id == constraintId));

            Publish(subscribers, message);
        }
       
        public override void Unsubscribe(object subscriber)
        {
            base.Unsubscribe(subscriber);
            if (eventProxy != null)
            {
                constraints.Remove(subscriber);
                var proxyEvents = GetProxyEventTypes(subscriber);
                eventProxy.Unsubscribe(proxyEvents, constraints[subscriber]);
            }
        }

        private IEnumerable<Type> GetProxyEventTypes(object subscriber)
        {
            var eventProxyType = typeof(TProxyEvent);
            var type = subscriber.GetType();
            var handleType = typeof(IHandle<>);
            return type.GetInterfaces()
                .Where(i => i.GUID == handleType.GUID && eventProxyType.IsAssignableFrom(i.GetGenericArguments()[0]))
                .Select(i => i.GetGenericArguments()[0]);
        }
    }
}
