using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Options;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.ProxyEvents;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions;
using Subscription = SignalR.EventAggregatorProxy.Client.DotNetCore.Model.Subscription;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public class EventAggregator : IEventAggregator
    {
        private readonly WeakReferenceList<object> subscribers = new WeakReferenceList<object>();

        public virtual void Subscribe(object subscriber)
        {
            subscribers.Add(subscriber);
        }

        public void Publish<T>(T message) where T : class
        {
            Publish(ListSubscribers(message), message);
        }

        protected virtual void Publish<T>(IEnumerable<IHandle<T>> filteredSubscribers,  T message) where T : class
        {
            filteredSubscribers
                .ForEach(s => s.Handle(message));
        }

        protected IEnumerable<IHandle<T>> ListSubscribers<T>(T message) where T : class
        {
            return subscribers
                .OfType<IHandle<T>>();
        } 

        public virtual void Unsubscribe(object subscriber)
        {
            subscribers.Remove(subscriber);
        }
    }

    internal class ProxyEventAggregator : EventAggregator, IProxyEventAggregator
    {
        private readonly EventProxy eventProxy;
        private readonly ITypeFinder typerFinder;
        private readonly ISubscriptionStore subscriptionStore;
        
        public ProxyEventAggregator(ISubscriptionStore subscriptionStore, EventProxy eventProxy, ITypeFinder typerFinder, IOptionsBuilder options)
        {
            this.subscriptionStore = subscriptionStore;
            this.eventProxy = eventProxy;
            this.typerFinder = typerFinder;

            options.ConfigureProxy(eventProxy, this);
        }

        public override void Subscribe(object subscriber)
        {
            Subscribe(subscriber, null);
        }

        public void Subscribe(object subscriber, Action<IConstraintinfoBuilder> buildConstraints)
        {
            base.Subscribe(subscriber);
            if (eventProxy != null)
            {
                List<IConstraintInfo> constraintInfos = new List<IConstraintInfo>();
                if (buildConstraints != null)
                {
                    var builder = new ConstraintinfoBuilder(constraintInfos, subscriptionStore);
                    buildConstraints(builder);
                }

                CheckSubscriberConstraints(constraintInfos);

                subscriptionStore.AddConstraints(subscriber, constraintInfos);
                var proxyEvents = GetProxyEventTypes(subscriber);

                var subscriptions = from pe in proxyEvents
                                    join ci in constraintInfos on pe equals ci.GetType().GetGenericArguments()[0] into eci
                            from ciOuter in eci.DefaultIfEmpty()
                                    select new Subscription(pe, ciOuter != null ? ciOuter.GetConstraint() : null, ciOuter.GetConstraintId());

                var actualSubscriptions = subscriptionStore.GetActualSubscriptions(subscriptions.ToList());

                eventProxy.Subscribe(actualSubscriptions);
            }
        }

        public void Publish<T>(T message, int? constraintId) where T : class
        {
            var subscribers = ListSubscribers(message)
                .Where(s => !constraintId.HasValue || subscriptionStore.HasConstraint(s, constraintId.Value));

            Publish(subscribers, message);
        }
       
        public override void Unsubscribe(object subscriber)
        {
            base.Unsubscribe(subscriber);
            if (eventProxy != null)
            {
                var proxyEvents = GetProxyEventTypes(subscriber);
                var actualUnsubscriptions = subscriptionStore.PopSubscriptions(proxyEvents, subscriber);
                eventProxy.Unsubscribe(actualUnsubscriptions).Wait();
            }
        }

        private IEnumerable<Type> GetProxyEventTypes(object subscriber)
        {
            return typerFinder.GetSubscriberEventTypes(subscriber);
        }

        private void CheckSubscriberConstraints(IEnumerable<IConstraintInfo> constraintInfos)
        {
            if(constraintInfos.GroupBy(c => c.Id).Any(c => c.Count() != 1)) throw new ArgumentException("One subscriber cant subscribe to the same constraint twice");
        }

    }
}
