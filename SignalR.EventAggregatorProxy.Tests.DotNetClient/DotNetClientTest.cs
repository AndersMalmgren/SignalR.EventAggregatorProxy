using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Model;


namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class DotNetClientTest : Test
    {
        protected Func<Task> reconnectedCallback;
        protected IProxyEventAggregator EventAggregator => Get<IProxyEventAggregator>();
        protected AutoResetEvent reset;

        private MulticastDelegate onEvent;
        private readonly PropertyInfo typeProp;
        private readonly Type messageType;
        private PropertyInfo genericProp;
        private PropertyInfo eventProp;
        private PropertyInfo idProp;

        protected DotNetClientTest()
        {
            messageType = typeof(EventProxy).Assembly.GetType("SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.EventProxy+Message");
            typeProp = messageType.GetProperty("Type");
            genericProp = messageType.GetProperty("GenericArguments");
            eventProp = messageType.GetProperty("Event");
            idProp = messageType.GetProperty("Id");
        }

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            Task<IHub> connectionFactory = null;

            var eventType = typeof(Event);
            var eventTypes = eventType.Assembly.GetTypes().Where(t => eventType.IsAssignableFrom(t)).ToList();


            reset = new AutoResetEvent(false);
            serviceCollection
                .AddSignalREventAggregator()
                .WithHubUrl(string.Empty)
                .OnSubscriptionError(OnFaultedSubscription)
                .OnConnected(OnConnected)
                .Build()
                .MockSingleton<IHub>(mock => 
                { 
                    mock.Setup(x => x.InvokeAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Callback((string method, object[] args, CancellationToken token) =>
                    {
                        if(method == "subscribe")
                            OnSubscribe(args[0] as IEnumerable<dynamic>, (bool)args[1]);
                        else
                            OnUnsubscribe(args[0] as IEnumerable<dynamic>);
                    }).Returns(Task.CompletedTask);

                    mock.Setup(x => x.On("onEvent", It.IsAny<Action<It.IsSubtype<object>>>())).Callback(
                        (string e, MulticastDelegate callback) =>
                        {
                            onEvent = callback;
                        });
                })
                .MockSingleton<IEventTypeFinder>(mock => mock.Setup(x => x.ListEventsTypes()).Returns(eventTypes))
                .MockSingleton<IHubProxyFactory>(mock => mock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Func<IHub, Task>>(), It.IsAny<Func<Task>>(), It.IsAny<Action<Exception>>(), It.IsAny<Action>(), It.IsAny<Action<HubConnection>>()))
                .Callback((string url, Func<IHub, Task> onstarted, Func<Task> onreconnected, Action<Exception> faulted, Action connected, Action<HubConnection> configure) =>
                {
                    connectionFactory = Task.Run(async () =>
                    {
                        var hub = Get<IHub>();

                        await onstarted(hub);
                        reconnectedCallback = onreconnected;
                        connected();
                        return hub;
                    });
                }).Returns(() => connectionFactory));
        }

        protected void PublishEvent<T>(T @event, object constraint = null)
        {
            var type = typeof(T);
            var message = Activator.CreateInstance(messageType!);
            
            typeProp.SetValue(message, type.GetFullNameWihoutGenerics());
            genericProp.SetValue(message, type.GetGenericArguments().Select(t => t.FullName).ToArray());
            eventProp.SetValue(message, JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(@event)).RootElement);

            int? id = null;
            if (constraint != null)
            {
                id = (type.FullName + JsonSerializer.Serialize(constraint)).GetHashCode();
            }


            idProp.SetValue(message, id);

            onEvent.Method.Invoke(onEvent.Target, new[] { message });
        }

        protected virtual void OnFaultedSubscription(Exception exception, IList<Subscription> subscriptions)
        {

        }

        protected virtual void OnConnected()
        {

        }

        protected virtual void OnUnsubscribe(IEnumerable<object> enumerable)
        {
            reset.Set();
        }

        protected virtual void OnSubscribe(IEnumerable<dynamic> subscriptions, bool reconnected)
        {
            reset.Set();
        }
    }
}
