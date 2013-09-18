using System;
using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.Extensions;
using SignalR.EventAggregatorProxy.Client.Model;

namespace SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents
{
    public class SubscriptionStore : ISubscriptionStore
    {
        private readonly Dictionary<Type, List<Subscription>> eventSubscriptions;
        private readonly Dictionary<Type, List<IConstraintInfo>> eventConstraints;
        private readonly Dictionary<object, IEnumerable<IConstraintInfo>> subscriberConstraints;

        private int constraintIdCounter;
        private readonly CompareObjects comparer;

        public SubscriptionStore()
        {
            eventSubscriptions = new Dictionary<Type, List<Subscription>>();
            eventConstraints = new Dictionary<Type, List<IConstraintInfo>>();
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

        public IEnumerable<Subscription> PopSubscriptions(IEnumerable<Type> eventTypes, object subscriber)
        {
            var actualUnsubscriptions = new List<Subscription>();

            foreach (var eventType in eventTypes)
            {
                var subscriptions = eventSubscriptions[eventType];

                var remove = subscriptions
                    .ToList()
                    .Where(s => !s.ConstraintId.HasValue || subscriberConstraints[subscriber].Any(c => c.Id == s.ConstraintId))
                    .GroupBy(s => s.ConstraintId)
                    .ForEach(g => subscriptions.Remove(g.First()))
                    .Where(g => g.Count() == 1).Select(g => g.First())
                    .ToList();

                actualUnsubscriptions.AddRange(remove);
                RemoveConstraint(eventType, remove);
            }

            subscriberConstraints.Remove(subscriber);

            return actualUnsubscriptions;
        }

        public void AddConstraints(object subscriber, IEnumerable<IConstraintInfo> constraints)
        {
            subscriberConstraints[subscriber] = constraints;
        }

        public bool HasConstraint(object subscriber, int constraintId)
        {
            return subscriberConstraints[subscriber].Any(c => c.Id == constraintId);
        }
        
        public int GenerateConstraintId<TEvent>(IConstraintInfo constraint)
        {
            var eventType = typeof (TEvent);

            if (!eventConstraints.ContainsKey(eventType))
                return GenerateNewConstraintId<TEvent>(constraint);

            
            var existing = eventConstraints[eventType].FirstOrDefault(c => comparer.Compare(constraint.GetConstraint(), c.GetConstraint()));
            return existing != null ? existing.Id : GenerateNewConstraintId<TEvent>(constraint);
        }

        private void RemoveConstraint(Type eventType, IEnumerable<Subscription> actualUnsubscriptions)
        {
            if(!eventConstraints.ContainsKey(eventType)) return;

            eventConstraints[eventType].RemoveAll(ec => actualUnsubscriptions.Any(s => ec.Id == s.ConstraintId));
            if (!eventConstraints[eventType].Any())
                eventConstraints.Remove(eventType);
        }


        private int GenerateNewConstraintId<TEvent>(IConstraintInfo constraint)
        {
            var eventType = typeof (TEvent);
            if(!eventConstraints.ContainsKey(eventType))
                eventConstraints[eventType] = new List<IConstraintInfo>();

            eventConstraints[eventType].Add(constraint);

            return constraintIdCounter++;
        }

        private void AddSubscription(Subscription subscription)
        {
            if(!eventSubscriptions.ContainsKey(subscription.EventType)) 
                eventSubscriptions[subscription.EventType] = new List<Subscription>();

            eventSubscriptions[subscription.EventType].Add(subscription);
        }

        private bool UniqueSubscription(Subscription subscription)
        {
            if (!eventSubscriptions.ContainsKey(subscription.EventType)) return true;

            if (subscription.Constraint == null) return false;

            return ConstraintUnique(subscription);
        }

        private bool ConstraintUnique(Subscription subscription)
        {
            return eventSubscriptions[subscription.EventType]
                .Where(s => s != subscription)
                .All(s => s.ConstraintId != subscription.ConstraintId);
        }
        
    }
}
