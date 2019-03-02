using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace SignalR.EventAggregatorProxy.Tests
{
    public abstract class Test
    {
        private readonly IServiceProvider serviceProvider;

        protected Test()
        {
            var col = new ServiceCollection();
            ConfigureCollection(col);
            serviceProvider = col.BuildServiceProvider();
        }

        protected virtual void ConfigureCollection(IServiceCollection serviceCollection)
        {
        }

        protected T Get<T>() where T : class
        {
            return serviceProvider.GetService<T>();
        }
    }
}
