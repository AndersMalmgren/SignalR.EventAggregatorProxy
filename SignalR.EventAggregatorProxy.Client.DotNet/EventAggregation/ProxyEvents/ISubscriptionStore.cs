using System;
using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Model;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents
{
    public interface ISubscriptionStore
    {
        IEnumerable<Subscription> GetActualSubscriptions(IEnumerable<Subscription> newSubscriptions);
        int GenerateConstraintId<TEvent>(IConstraintInfo constraint);
        void AddConstraints(object subscriber, IEnumerable<IConstraintInfo> constraints);
        bool HasConstraint(object subscriber, int constraintId);
        IEnumerable<Subscription> PopSubscriptions(IEnumerable<Type> eventTypes, object subscriber);
        IEnumerable<Subscription> ListUniqueSubscriptions();
    }
}