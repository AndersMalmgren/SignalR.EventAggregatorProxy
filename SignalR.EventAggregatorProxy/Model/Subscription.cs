using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SignalR.EventAggregatorProxy.Model
{
    public class Subscription
    {
        public Subscription(Type eventType, string connectionId, string username, JsonElement constraint, int? constraintId, IList<Type> genericArguments)
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
        public JsonElement Constraint { get; set; }
        public IList<Type> GenericArguments { get; set; }
        public int? ConstraintId { get; set; }
    }
}

