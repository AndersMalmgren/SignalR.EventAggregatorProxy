using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace SignalR.EventAggregatorProxy.Tests
{
    public static class MockExtensions
    {
        public static IServiceCollection MockSingleton<T>(this IServiceCollection collection, Action<Mock<T>> setup = null) where T : class
        {
            collection.AddSingleton(Mock(setup));
            return collection;
        }

        public static IServiceCollection MockTransiant<T>(this IServiceCollection collection, Action<Mock<T>> setup = null) where T : class
        {
            collection.AddTransient(p => Mock(setup));
            return collection;
        }

        private static T Mock<T>(Action<Mock<T>> setup = null) where T : class
        {
            var mock = new Mock<T>();
            setup?.Invoke(mock);
            return mock.Object;
        }
    }
}
