using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Model;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public interface ISubscriptionStore
    {
        IEnumerable<Subscription> GetActualSubscriptions(IEnumerable<Subscription> subscriptions);
        int GenerateConstraintId<TEvent>(IConstraintInfo constraint);
        void AddSubscriberConstraints(object subscriber, IEnumerable<IConstraintInfo> constraints);
        bool HasConstraint(object subscriber, int constraintId);
        IEnumerable<IConstraintInfo> PopSubscriberConstraints(object subcriber);
    }
}