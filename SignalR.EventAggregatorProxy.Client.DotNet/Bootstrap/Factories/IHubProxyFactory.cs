using System;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap.Factories
{
    public interface IHubProxyFactory
    {
        IHubProxy Create(string hubUrl, Action<IHubConnection> configureConnection, Action<IHubProxy> onStarted);
    }
}