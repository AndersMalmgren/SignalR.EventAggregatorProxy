namespace SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation
{
    public interface IHandle<in T> where T : class
    {
        void Handle(T message);
    }
}
