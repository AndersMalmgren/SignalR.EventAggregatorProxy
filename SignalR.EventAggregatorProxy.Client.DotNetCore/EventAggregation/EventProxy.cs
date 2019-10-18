using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.ProxyEvents;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions;
using Subscription = SignalR.EventAggregatorProxy.Client.DotNetCore.Model.Subscription;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public class EventProxy
    {
        private IProxyEventAggregator eventAggregator;
        private readonly IHubProxyFactory hubProxyFactory;
        private Action<Exception> faultedConnectingAction;
        private Action<Exception, IList<Subscription>> faultedSubscriptionAction;
        private Action connectedAction;
        private bool queueSubscriptions = true;
        private readonly List<Subscription> subscriptionQueue;
        private IHub proxy;
        private readonly ITypeFinder typeFinder;
        private readonly ISubscriptionThrottleHandler throttleHandler;
        private readonly ISubscriptionStore subscriptionStore;

        public EventProxy(IHubProxyFactory hubProxyFactory, ISubscriptionThrottleHandler throttleHandler, ISubscriptionStore subscriptionStore, ITypeFinder typeFinder)
        {
            this.typeFinder = typeFinder;
            subscriptionQueue = new List<Subscription>();

            this.hubProxyFactory = hubProxyFactory;

            this.throttleHandler = throttleHandler;
            throttleHandler.Init(() => SendQueuedSubscriptions());
            this.subscriptionStore = subscriptionStore;
        }

        public async Task Init(string hubUrl, IProxyEventAggregator eventAggregator, Action<HubConnection> configureConnection, Action<Exception> faultedConnectingAction, Action<Exception, IList<Subscription>> faultedSubscriptionAction, Action connectedAction)
        {
            this.eventAggregator = eventAggregator;
            this.faultedConnectingAction = faultedConnectingAction;
            this.faultedSubscriptionAction = faultedSubscriptionAction;
            this.connectedAction = connectedAction;

            await hubProxyFactory
                .Create(hubUrl, configureConnection, async p =>
                    {
                        proxy = p;
                        await SendQueuedSubscriptions();
                        p.On<Message>("onEvent", OnEvent);
                    },
                    Reconnected, FaultedConnection, ConnectionComplete);
        }

        public void Subscribe(IEnumerable<Subscription> subscriptions)
        {
            lock (subscriptionQueue)
                subscriptionQueue.AddRange(subscriptions);

            if (!queueSubscriptions)
            {
                throttleHandler.Throttle();
            }
        }

        private void OnEvent(Message message)
        {   
            var @event = ParseTypeData(message);
            eventAggregator.Publish(@event, message.Id);
        }

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true};

        private dynamic ParseTypeData(Message message)
        {
            Type type = typeFinder.GetEventType(message.Type);
            if (message.GenericArguments.Length > 0)
            {
                var genericArguments = message.GenericArguments
                    .Select(typeFinder.GetType)
                    .ToArray();

                type = type.MakeGenericType(genericArguments);
            }

            var json = message.Event.GetRawText();
            return JsonSerializer.Deserialize(json, type, Options);
        }
        
        public async Task Unsubscribe(IEnumerable<Subscription> subscriptions)
        {
            var unsubsscriptions = subscriptions.Select(s => new { type = s.EventType.GetFullNameWihoutGenerics(), genericArguments = s.EventType.GetGenericArguments().Select(ga => ga.FullName), id = s.ConstraintId }).ToList();
            if (unsubsscriptions.Any())
            {
                queueSubscriptions = true;
                await proxy.InvokeAsync("unsubscribe", new object[] { unsubsscriptions});
                await SendQueuedSubscriptions();
            }
        }

        private async Task SendQueuedSubscriptions(bool reconnected = false)
        {
            try
            {
                queueSubscriptions = false;

                IEnumerable<object> subscriptions;
                lock (subscriptionQueue)
                {
                    subscriptions = subscriptionQueue.Select(s => new
                    {
                        Type = s.EventType.GetFullNameWihoutGenerics(),
                        GenericArguments = s.EventType.GetGenericArguments().Select(ga => ga.FullName),
                        s.Constraint,
                        s.ConstraintId
                    }).ToList();
                    subscriptionQueue.Clear();
                }

                if (subscriptions.Any())
                    await proxy.InvokeAsync("subscribe", new object[] { subscriptions, reconnected });
            }
            catch (Exception ex)
            {
                FaultedSendingQueuedSubscriptions(ex);
            }
        }

        private void FaultedSendingQueuedSubscriptions(Exception ex)
        {
            faultedSubscriptionAction?.Invoke(ex, new List<Subscription>(subscriptionQueue));
        }

        private void ConnectionComplete()
        {
            connectedAction?.Invoke();
        }

        private Task Reconnected()
        {
            connectedAction();

            subscriptionQueue.AddRange(subscriptionStore.ListUniqueSubscriptions());
            return  SendQueuedSubscriptions(true);
        }

        private void FaultedConnection(Exception ex)
        {
            /* Since we are using tasks, most of the time we will get an AggregateException with only one InnerException */
            var aggregateException = ex as AggregateException;

            if (aggregateException != null && aggregateException.InnerExceptions.Count == 1)
                ex = aggregateException.InnerException;
         
            if (faultedConnectingAction != null)
                faultedConnectingAction(ex);

            if (subscriptionQueue.Count != 0)
                FaultedSendingQueuedSubscriptions(ex);
        }
        
        private class Message
        {
            public string Type { get; set; }
            public JsonElement Event { get; set; }
            public string[] GenericArguments { get; set; }
            public int? Id { get; set; }
        }

    }
}
