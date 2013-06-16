namespace SignalR.EventAggregatorProxy.Client.EventAggregation
{
    public interface IEventAggregator<TProxyEvent>
    {
        void Subscribe(object subsriber);
        void Publish<T>(T message) where T : class;
    }
}
