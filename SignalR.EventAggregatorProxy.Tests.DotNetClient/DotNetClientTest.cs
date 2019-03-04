using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Model;


namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class DotNetClientTest : Test
    {
        protected Func<Task> reconnectedCallback;
        protected IProxyEventAggregator EventAggregator => Get<IProxyEventAggregator>();
        protected AutoResetEvent reset;

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            Task<IHub> connectionFactory = null;

            var eventType = typeof(Event);
            var eventTypes = eventType.Assembly.GetTypes().Where(t => eventType.IsAssignableFrom(t)).ToList();


            reset = new AutoResetEvent(false);
            serviceCollection
                .AddSignalREventAggregator()
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
                })
                .MockSingleton<IEventTypeFinder>(mock => mock.Setup(x => x.ListEventsTypes()).Returns(eventTypes))
                .MockSingleton<IHubProxyFactory>(mock => mock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Action<HubConnection>>(), It.IsAny<Func<IHub, Task>>(), It.IsAny<Func<Task>>(), It.IsAny<Action<Exception>>(), It.IsAny<Action>()))
                .Callback((string url, Action<HubConnection> configure, Func<IHub, Task> onstarted, Func<Task> onreconnected, Action<Exception> faulted, Action connected) =>
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
