using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Client.Model
{
    public class Subscription
    {
        public Subscription(Type eventType, object constraint, int? constraintId)
        {
            EventType = eventType;
            Constraint = constraint;
            ConstraintId = constraintId;
        }

        public Type EventType { get; set; }
        public object Constraint { get; set; }
        public int? ConstraintId { get; set; }
    }
}
