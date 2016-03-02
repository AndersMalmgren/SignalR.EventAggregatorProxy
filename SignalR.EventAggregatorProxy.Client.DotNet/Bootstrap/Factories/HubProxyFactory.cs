using System;
using System.Diagnostics;
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

            var isConnected = false;

            Action start = () =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await connection.Start();
                        if(isConnected)
                            reconnected();
                        else
                        {
                            isConnected = true;
                            onStarted(proxy);
                            connected();
                        }
                    }
                    catch(Exception ex)
                    {
                        faulted(ex);
                    }
                });
            };

            connection.Closed += start;

            start();

            return proxy;
        }
    }
}
