using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Extensions;
using SignalR.EventAggregatorProxy.Client.Model;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public class SubscriptionStore : ISubscriptionStore
    {
        private readonly Dictionary<Type, List<Subscription>> subscriptions;
        private readonly Dictionary<Type, List<IConstraintInfo>> constraints;
        private int constraintIdCounter;

        public SubscriptionStore()
        {
            subscriptions = new Dictionary<Type, List<Subscription>>();
            constraints = new Dictionary<Type, List<IConstraintInfo>>();
        }

        public IEnumerable<Subscription> GetActualSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            var uniqueSubscription = subscriptions
                .Where(UniqueSubscription)
                .ToList();

            subscriptions.ForEach(AddSubscription);

            return uniqueSubscription;
        }

        public int GenerateConstraintId<TEvent>(IConstraintInfo constraint)
        {
            var eventType = typeof (TEvent);

            if (!constraints.ContainsKey(eventType))
                return GenerateNewConstraintId<TEvent>(constraint);

            var comparer = new CompareObjects();
            var existing = constraints[eventType].FirstOrDefault(c => comparer.Compare(constraint.GetConstraint(), c.GetConstraint()));
            return existing != null ? existing.Id : GenerateNewConstraintId<TEvent>(constraint);
        }

        private int GenerateNewConstraintId<TEvent>(IConstraintInfo constraint)
        {
            var eventType = typeof (TEvent);
            if(!constraints.ContainsKey(eventType))
                constraints[eventType] = new List<IConstraintInfo>();

            constraints[eventType].Add(constraint);

            return constraintIdCounter++;
        }

        private void AddSubscription(Subscription subscription)
        {
            if(!subscriptions.ContainsKey(subscription.EventType)) 
                subscriptions[subscription.EventType] = new List<Subscription>();

            subscriptions[subscription.EventType].Add(subscription);
        }

        private bool UniqueSubscription(Subscription subscription)
        {
            if (!subscriptions.ContainsKey(subscription.EventType)) return true;

            if (subscription.Constraint == null) return false;

            return ConstraintUnique(subscription);
        }

        private bool ConstraintUnique(Subscription subscription)
        {
            return subscriptions[subscription.EventType]
                .Where(s => s != subscription)
                .All(s => s.ConstraintId != subscription.ConstraintId);
        }
        
    }
}
