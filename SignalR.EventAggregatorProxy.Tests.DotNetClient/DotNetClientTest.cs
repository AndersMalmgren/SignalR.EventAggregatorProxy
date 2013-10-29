using SignalR.EventAggregatorProxy.Client.Bootstrap;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class DotNetClientTest : Test
    {
        protected override void Reset()
        {
        }

        public override T Get<T>()
        {
            return DependencyResolver.Global.Get<T>();
        }

        public override void Register<T>(T stub)
        {
            DependencyResolver.Global.Register(() => stub);
        }
    }
}
