namespace SignalR.EventAggregatorProxy.Demo.Contracts.Events
{
    public class ConstrainedEvent : StandardEvent
    {
        public ConstrainedEvent(string message) : base(message)
        {
        }
    }
}