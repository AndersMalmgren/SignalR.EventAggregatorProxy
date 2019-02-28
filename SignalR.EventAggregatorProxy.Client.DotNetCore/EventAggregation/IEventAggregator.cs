using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public interface IProxyEventAggregator : IEventAggregator
    {
        void Subscribe(object subscriber, Action<IConstraintinfoBuilder> buildConstraints);
        void Publish<T>(T message, int? constraintId) where T : class;
    }

    public interface IEventAggregator
    {
        void Subscribe(object subsriber);
        void Publish<T>(T message) where T : class;
        void Unsubscribe(object subscriber);
    }
}
