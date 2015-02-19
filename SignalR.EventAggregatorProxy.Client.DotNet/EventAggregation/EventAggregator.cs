using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Client.Hubs;
using SignalR.EventAggregatorProxy.Client.Bootstrap;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents;
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
        private readonly ISubscriptionStore subscriptionStore;

        public EventAggregator()
        {
            subscriptionStore = DependencyResolver.Global.Get<ISubscriptionStore>();
        }

        public EventAggregator<TProxyEvent> Init(string hubUrl, Action<IHubConnection> configureConnection = null)
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
                CheckSubscriberConstraints(constraintInfos);

                subscriptionStore.AddConstraints(subscriber, constraintInfos);
                var proxyEvents = GetProxyEventTypes(subscriber);

                var subscriptions = from pe in proxyEvents
                                    join ci in constraintInfos on pe equals ci.GetType().GetGenericArguments()[0] into eci
                            from ciOuter in eci.DefaultIfEmpty()
                                    select new Subscription(pe, ciOuter != null ? ciOuter.GetConstraint() : null, ciOuter.GetConstraintId());

                var actualSubscriptions = subscriptionStore.GetActualSubscriptions(subscriptions.ToList());

                eventProxy.Subscribe(actualSubscriptions);
            }
        }

        public void Publish<T>(T message, int? constraintId) where T : class
        {
            var subscribers = ListSubscribers(message)
                .Where(s => !constraintId.HasValue || subscriptionStore.HasConstraint(s, constraintId.Value));

            Publish(subscribers, message);
        }
       
        public override void Unsubscribe(object subscriber)
        {
            base.Unsubscribe(subscriber);
            if (eventProxy != null)
            {
                var proxyEvents = GetProxyEventTypes(subscriber);
                var actualUnsubscriptions = subscriptionStore.PopSubscriptions(proxyEvents, subscriber);
                eventProxy.Unsubscribe(actualUnsubscriptions);
            }
        }

        private IEnumerable<Type> GetProxyEventTypes(object subscriber)
        {
            var eventProxyType = typeof(TProxyEvent);
            var type = subscriber.GetType();
            var handleType = typeof(IHandle<>);
            return type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handleType && eventProxyType.IsAssignableFrom(i.GetGenericArguments()[0]))
                .Select(i => i.GetGenericArguments()[0]);
        }

        private void CheckSubscriberConstraints(IEnumerable<IConstraintInfo> constraintInfos)
        {
            try
            {
                constraintInfos.GroupBy(c => c.Id).Select(c => c.Single()).ToList();
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("One subscriber cant subscribe to the same constraint twice");
            }
        }

    }
}
