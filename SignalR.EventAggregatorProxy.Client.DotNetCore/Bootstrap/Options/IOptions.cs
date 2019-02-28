using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
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
}
