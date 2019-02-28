using System;
using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Model;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.ProxyEvents
{
    public interface ISubscriptionStore
    {
        IEnumerable<Subscription> GetActualSubscriptions(IEnumerable<Subscription> newSubscriptions);
        void AddConstraints(object subscriber, IEnumerable<IConstraintInfo> constraints);
        bool HasConstraint(object subscriber, int constraintId);
        IEnumerable<Subscription> PopSubscriptions(IEnumerable<Type> eventTypes, object subscriber);
        IEnumerable<Subscription> ListUniqueSubscriptions();
        void AddConstraint<TEvent, TConstraint>(ConstraintInfo<TEvent, TConstraint> constraintInfo);
    }
}