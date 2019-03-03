using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

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
                }
            };


            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000); //Best practice acccording to demo :P
                await start();
            };

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
            void InternalHandler(object[] args) => handler((T)args[0]);

            return connection.On(methodName, new[] { typeof(T) }, (parameters, state) =>
            {
                var currentHandler = (Action<object[]>)state;
                currentHandler(parameters);
                return Task.CompletedTask;
            }, (Action<object[]>)InternalHandler);
        }

        public Task InvokeAsync(string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            return connection.InvokeCoreAsync(methodName, typeof(object), args, cancellationToken);
        }
    }
}
