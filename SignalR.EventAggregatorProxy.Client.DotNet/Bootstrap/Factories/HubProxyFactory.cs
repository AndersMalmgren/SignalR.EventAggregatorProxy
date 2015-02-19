using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap.Factories
{
    public class HubProxyFactory : IHubProxyFactory
    {
        public IHubProxy Create(string hubUrl, Action<IHubConnection> configureConnection, Action<IHubProxy> onStarted, Action reconnected)
        {
            var connection = new HubConnection(hubUrl);
            if (configureConnection != null)
                configureConnection(connection);

            var proxy = connection.CreateHubProxy("EventAggregatorProxyHub");
            var connectionTask = connection.Start().ContinueWith(o => onStarted(proxy), TaskContinuationOptions.NotOnFaulted);
            connection.Start().ContinueWith(o => onStarted(proxy));
            connection.Reconnected += reconnected;

            connectionTask.Wait();

            if (connectionTask.Exception == null) return proxy;

            if (connectionTask.Exception.InnerExceptions.Count == 1)
            {
                throw connectionTask.Exception.InnerException;
            }

            throw connectionTask.Exception;
        }
    }
}
