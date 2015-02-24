using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap.Factories
{
    public class HubProxyFactory : IHubProxyFactory
    {
        public IHubProxy Create(string hubUrl, Action<IHubConnection> configureConnection, Action<IHubProxy> onStarted, Action reconnected, Action<Exception> faulted, Action connected)
        {
            var connection = new HubConnection(hubUrl);
            if (configureConnection != null)
                configureConnection(connection);

            var proxy = connection.CreateHubProxy("EventAggregatorProxyHub");
            connection.Reconnected += reconnected;
            connection.Error += faulted;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    connection.Start().Wait();
                    onStarted(proxy);
                    connected();
                }
                catch (Exception ex)
                {
                    faulted(ex);
                }
            });

            return proxy;
        }
    }
}
