using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Model;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Bootstrap.Options
{
    internal class OptionsBuilder : IOptionsBuilder
    {
        private IServiceCollection? collection;
        private Action<Exception>? faultedConnectingAction;
        private Action<Exception, IList<Subscription>>? faultedSubscriptionAction;
        private Action? connectedAction;
        private string? hubUrl;
        private Action<HubConnection>? configureConnection;

        public OptionsBuilder(IServiceCollection collection)
        {
            this.collection = collection;
        }

        public IOptions WithHubUrl(string hubUrl)
        {
            EnsureBuild();
            this.hubUrl = hubUrl;
            return this;
        }

        public IOptions OnConnectionError(Action<Exception> faultedConnectingAction)
        {
            EnsureBuild();
            this.faultedConnectingAction = faultedConnectingAction;
            return this;
        }

        public IOptions OnSubscriptionError(Action<Exception, IList<Subscription>> faultedSubscriptionAction)
        {
            EnsureBuild();
            this.faultedSubscriptionAction = faultedSubscriptionAction;
            return this;
        }

        public IOptions OnConnected(Action connectedAction)
        {
            EnsureBuild();
            this.connectedAction = connectedAction;
            return this;
        }

        public IOptions ConfigureConnection(Action<HubConnection> configureConnection)
        {
            EnsureBuild();
            this.configureConnection = configureConnection;
            return this;
        }

        public IServiceCollection Build()
        {
            EnsureBuild();
            var col = collection.NotNull();
            collection = null;
            return col;
        }

        private void EnsureBuild()
        {
            if(collection == null)
                throw new Exception("Can not build options after calling Build");
        }

        public void ConfigureProxy(EventProxy eventProxy, IProxyEventAggregator eventAggregator)
        {
            Task.Run(() => eventProxy.Init(hubUrl.NotNull(), eventAggregator, configureConnection, faultedConnectingAction, faultedSubscriptionAction, connectedAction));
        }
    }
}
