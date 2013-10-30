using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap.Factories
{
    public class HubProxyFactory : IHubProxyFactory
    {
        public IHubProxy Create(string hubUrl, Action<IHubConnection> configureConnection, Action<IHubProxy> onStarted)
        {
            var connection = new HubConnection(hubUrl);
            if (configureConnection != null)
                configureConnection(connection);

            var proxy = connection.CreateHubProxy("EventAggregatorProxyHub");
            connection.Start().ContinueWith(o => onStarted(proxy));

            return proxy;
        }
    }
}
