using System;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Model
{
    public class Subscription
    {
        public Subscription(Type eventType, object? constraint, int? constraintId)
        {
            EventType = eventType;
            Constraint = constraint;
            ConstraintId = constraintId;
        }

        public Type EventType { get; set; }
        public object? Constraint { get; set; }
        public int? ConstraintId { get; set; }
    }
}
