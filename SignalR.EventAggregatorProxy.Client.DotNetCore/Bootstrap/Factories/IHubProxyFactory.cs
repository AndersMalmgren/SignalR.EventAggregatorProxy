using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Factories
{
    public interface IHubProxyFactory
    {
        Task<HubConnection> Create(string hubUrl, Action<HubConnection> configureConnection, Func<HubConnection, Task> onStarted, Func<Task> reconnected, Action<Exception> faulted, Action connected);
    }
}