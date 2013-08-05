using System;
using System.Collections.Generic;

namespace SignalR.EventAggregatorProxy.Model
{
    public class Subscription
    {
        public Subscription(Type eventType, string connectionId, string username, dynamic constraint, int? constraintId, IList<Type> genericArguments)
        {
            EventType = eventType;
            ConnectionId = connectionId;
            Username = username;
            Constraint = constraint;
            GenericArguments = genericArguments;
            ConstraintId = constraintId;
        }

        public Type EventType { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public dynamic Constraint { get; set; }
        public IList<Type> GenericArguments { get; set; }
        public int? ConstraintId { get; set; }
    }
}

