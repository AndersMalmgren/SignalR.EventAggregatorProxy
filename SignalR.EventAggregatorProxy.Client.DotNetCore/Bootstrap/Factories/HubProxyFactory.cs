using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories
{
    public class HubProxyFactory : IHubProxyFactory
    {
        public async Task<IHub> Create(string hubUrl, Action<HubConnection> configureConnection, Func<IHub, Task> onStarted, Func<Task> reconnected, Action<Exception> faulted, Action connected)
        {
            var isConnected = false;

            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();
            var hub = new Hub(connection);

            configureConnection?.Invoke(connection);

            Func<int, Task> delayedStart = null;

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
                        await onStarted(hub);
                        connected();
                    }
                }
                catch (Exception ex)
                {
                    faulted(ex);
                    await delayedStart(5000);
                }
            };

            delayedStart = async delay =>
            {
                await Task.Delay(delay); 
                await start();
            };
            
            connection.Closed += async (error) => await delayedStart(new Random().Next(0, 5) * 1000); //Best practice acccording to demo :P

            await start();
            return hub;
        }
    }

    public class Hub : IHub
    {
        private readonly HubConnection connection;

        public Hub(HubConnection connection)
        {
            this.connection = connection;
        }

        public IDisposable On<T>(string methodName, Action<T> handler)
        {
            return connection.On(methodName, handler);
        }

        public Task InvokeAsync(string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            return connection.InvokeCoreAsync(methodName, typeof(object), args, cancellationToken);
        }
    }
}
