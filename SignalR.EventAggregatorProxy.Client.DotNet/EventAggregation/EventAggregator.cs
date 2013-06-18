using System;
using System.Collections.Generic;
using System.Linq;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Extensions;

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
            subscribers
                .OfType<IHandle<T>>()
                .ForEach(s => s.Handle(message));
        }

        public virtual void Ubsubscribe(object subscriber)
        {
            subscribers.Remove(subscriber);
        }
    }

    public class EventAggregator<TProxyEvent> : EventAggregator, IEventAggregator<TProxyEvent>
    {
        private EventProxy<TProxyEvent> eventProxy;

        public EventAggregator<TProxyEvent> Init(string hubUrl)
        {
            eventProxy = new EventProxy<TProxyEvent>(this, hubUrl);
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
                var proxyEvents = GetProxyEventTypes(subscriber);
                foreach (var proxyEvent in proxyEvents)
                {
                    var constraint = constraintInfos.FirstOrDefault(ci => ci.GetType().GetGenericArguments()[0] == proxyEvent);
                    eventProxy.Subscribe(proxyEvent, constraint != null ? constraint.GetConstraint() : null);
                }
            }
        }

        public override void Ubsubscribe(object subscriber)
        {
            base.Ubsubscribe(subscriber);

            if (eventProxy != null)
            {
                var proxyEvents = GetProxyEventTypes(subscriber);
                eventProxy.Unsubscribe(proxyEvents);
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
