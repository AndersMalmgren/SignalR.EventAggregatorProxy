namespace SignalR.EventAggregatorProxy.Demo.Contracts.Events
{
    public class GenericEvent<T> : Event, IMessageEvent<T>
    {
        public T Message { get; set; }

        public GenericEvent() { }

        public GenericEvent(T message)
        {
            Message = message;
        }

        public string GetText()
        {
            return Message.ToString();
        }
    }
}