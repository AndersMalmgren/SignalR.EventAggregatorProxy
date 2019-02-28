namespace SignalR.EventAggregatorProxy.Demo.Contracts.Events
{
    public class StandardEvent : Event, IMessageEvent<string>
    {
        public string Message { get; set; }

        public StandardEvent(string message)
        {
            Message = message;
        }
    }
}