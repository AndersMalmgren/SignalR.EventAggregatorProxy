using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories
{
    public interface IHubProxyFactory
    {
        Task<IHub> Create(string hubUrl, Func<IHub, Task> onStarted, Func<Task> reconnected, Action<Exception> faulted, Action connected, Action<HubConnection>? configureConnection);
    }

    public interface IHub
    {
        IDisposable On<T>(string methodName, Action<T> handler);
        Task InvokeAsync(string methodName, object[] args, CancellationToken cancellationToken = default);
    }
}