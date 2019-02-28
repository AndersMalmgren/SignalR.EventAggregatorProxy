using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories
{
    public class HubProxyFactory : IHubProxyFactory
    {
        public async Task<HubConnection> Create(string hubUrl, Action<HubConnection> configureConnection, Func<HubConnection, Task> onStarted, Func<Task> reconnected, Action<Exception> faulted, Action connected)
        {
            var isConnected = false;

            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            configureConnection?.Invoke(connection);

            Func<Task> start = async () =>
            {
                

                try
                {
                    await connection.StartAsync();
                    if (isConnected)
                        await reconnected();
                    else
                    {
                        isConnected = true;
                        await onStarted(connection);
                        connected();
                    }
                }
                catch (Exception ex)
                {
                    faulted(ex);
                }
            };


            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000); //Best practice acccording to demo :P
                await start();
            };

            await start();
            return connection;
        }
    }
}
