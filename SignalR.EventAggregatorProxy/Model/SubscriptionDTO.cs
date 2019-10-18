using System.Text.Json;

namespace SignalR.EventAggregatorProxy.Model
{
    public class SubscriptionDto
    {
        public string Type { get; set; }
        public string[] GenericArguments { get; set; }
        public JsonElement Constraint { get; set; }
        public int? ConstraintId { get; set; }
    }
}
