using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.Constraint;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public interface IEventAggregator<TProxyEvent> : IEventAggregator
    {
        void Subscribe(object subscriber, IEnumerable<IConstraintInfo> constraintInfos);
    }

    public interface IEventAggregator
    {
        void Subscribe(object subsriber);
        void Publish<T>(T message) where T : class;
        void Ubsubscribe(object subscriber);
    }
}
