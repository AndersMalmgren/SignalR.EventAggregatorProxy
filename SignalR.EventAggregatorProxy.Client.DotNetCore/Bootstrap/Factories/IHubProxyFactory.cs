using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories
{
    public interface IHubProxyFactory
    {
        Task<IHub> Create(string hubUrl, Action<HubConnection> configureConnection, Func<IHub, Task> onStarted, Func<Task> reconnected, Action<Exception> faulted, Action connected);
    }

    public interface IHub
    {
        IDisposable On<T>(string methodName, Action<T> handler);
        Task InvokeAsync(string methodName, object arg, CancellationToken cancellationToken = default(CancellationToken));
        Task InvokeAsync(string methodName, object arg1, object arg2, CancellationToken cancellationToken = default(CancellationToken));
    }
}