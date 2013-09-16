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
        private readonly Dictionary<object, IEnumerable<IConstraintInfo>> subscriberConstraints;

        private int constraintIdCounter;
        private readonly CompareObjects comparer;

        public SubscriptionStore()
        {
            subscriptions = new Dictionary<Type, List<Subscription>>();
            constraints = new Dictionary<Type, List<IConstraintInfo>>();
            subscriberConstraints = new Dictionary<object, IEnumerable<IConstraintInfo>>();

            comparer = new CompareObjects();
        }

        public IEnumerable<Subscription> GetActualSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            var uniqueSubscription = subscriptions
                .Where(UniqueSubscription)
                .ToList();

            subscriptions.ForEach(AddSubscription);

            return uniqueSubscription;
        }

        public IEnumerable<Subscription> GetActualUnsubscriptions(IEnumerable<Subscription> subscriptions)
        {
            var uniqueSubscription = subscriptions
                .Where(UniqueSubscription)
                .ToList();

            subscriptions.ForEach(AddSubscription);

            return uniqueSubscription;
        }

        //public IEnumerable<Subscription> PopSubscriptions(IEnumerable<Type> eventTypes, object sbscriber)
        //{
        //    foreach (var eventType in eventTypes)
        //    {

        //    }
        //}

        public void AddSubscriberConstraints(object subscriber, IEnumerable<IConstraintInfo> constraints)
        {
            subscriberConstraints[subscriber] = constraints;
        }

        public bool HasConstraint(object subscriber, int constraintId)
        {
            return subscriberConstraints[subscriber].Any(c => c.Id == constraintId);
        }

        public IEnumerable<IConstraintInfo> PopSubscriberConstraints(object subcriber)
        {
            var constraint = subscriberConstraints[subcriber];
            subscriberConstraints.Remove(subcriber);
            return constraint;
        }

        public int GenerateConstraintId<TEvent>(IConstraintInfo constraint)
        {
            var eventType = typeof (TEvent);

            if (!constraints.ContainsKey(eventType))
                return GenerateNewConstraintId<TEvent>(constraint);

            
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
