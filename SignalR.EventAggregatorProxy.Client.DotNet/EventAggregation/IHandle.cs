namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public interface IHandle<in T> where T : class
    {
        void Handle(T message);
    }
}
