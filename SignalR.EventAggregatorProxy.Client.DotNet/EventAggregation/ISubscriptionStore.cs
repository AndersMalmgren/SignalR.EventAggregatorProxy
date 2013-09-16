using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Model;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public interface ISubscriptionStore
    {
        IEnumerable<Subscription> GetActualSubscriptions(IEnumerable<Subscription> subscriptions);
        int GenerateConstraintId<TEvent>(IConstraintInfo constraint);
    }
}