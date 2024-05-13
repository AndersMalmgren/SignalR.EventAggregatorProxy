namespace SignalR.EventAggregatorProxy.Model
{
    public class EventType
    {
        public required string Type { get; set; }
        public string[]? GenericArguments { get; set; }
        public int? ConstraintId { get; set; }
    }
}
