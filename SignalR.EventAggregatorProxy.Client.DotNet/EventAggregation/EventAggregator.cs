using System.Collections.Generic;
using System.Linq;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Extensions;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class EventAggregator<TProxyEvent> : IEventAggregator<TProxyEvent>
    {
        private readonly WeakReferenceList<object> subscribers = new WeakReferenceList<object>();
        private EventProxy<TProxyEvent> eventProxy;

        public EventAggregator<TProxyEvent> Init(string hubUrl)
        {
            eventProxy = new EventProxy<TProxyEvent>(this, hubUrl);
            return this;
        }

        public void Subscribe(object subscriber)
        {
            Subscribe(subscriber, new List<IConstraintInfo>());
        }

        public void Subscribe(object subscriber, IEnumerable<IConstraintInfo> constraintInfos)
        {
            subscribers.Add(subscriber);
            if (eventProxy != null)
            {
                var eventProxyType = typeof(TProxyEvent);
                var type = subscriber.GetType();
                var handleType = typeof(IHandle<>);
                var proxyEvents = type.GetInterfaces()
                    .Where(i => i.GUID == handleType.GUID && eventProxyType.IsAssignableFrom(i.GetGenericArguments()[0]))
                    .Select(i => i.GetGenericArguments()[0]);

                foreach (var proxyEvent in proxyEvents)
                {
                    var constraint = constraintInfos.FirstOrDefault(ci => ci.GetType().GetGenericArguments()[0] == proxyEvent);
                    eventProxy.Subscribe(proxyEvent, constraint != null ? constraint.GetConstraint() : null);
                }
            }
        }

        public void Publish<T>(T message) where T : class
        {
            subscribers
                .OfType<IHandle<T>>()
                .ForEach(s => s.Handle(message));
        }
    }
}
