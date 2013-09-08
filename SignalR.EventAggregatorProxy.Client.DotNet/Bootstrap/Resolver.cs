using System;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap
{
    internal class Resolver<T> : IResolver
    {
        private readonly Func<T> factory;

        public Resolver(Func<T> factory)
        {
            this.factory = factory;
        }

        public object Resolve()
        {
            return factory();
        }
    }
}