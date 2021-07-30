using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Model;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Options
{
    public interface IOptions
    {
        IOptions WithHubUrl(string hubUrl);
        IOptions OnConnectionError(Action<Exception> faultedConnectingAction);
        IOptions OnSubscriptionError(Action<Exception, IList<Subscription>> faultedSubscriptionAction);
        IOptions OnConnected(Action connectedAction);
        IOptions ConfigureConnection(Action<HubConnection> configureConnection);
        IServiceCollection Build();
    }

    internal interface IOptionsBuilder : IOptions
    {
        void ConfigureProxy(EventProxy eventProxy, IProxyEventAggregator eventAggregator);
    }
}
