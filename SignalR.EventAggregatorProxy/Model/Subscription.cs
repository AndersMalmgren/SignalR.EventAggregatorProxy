using System;
using System.Collections.Generic;

namespace SignalR.EventAggregatorProxy.Model
{
    public class Subscription
    {
        public Subscription(string connectionId, string username, dynamic constraint, IList<Type> genericArguments)
        {
            ConnectionId = connectionId;
            Username = username;
            Constraint = constraint;
            GenericArguments = genericArguments;
        }

        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public dynamic Constraint { get; set; }
        public IList<Type> GenericArguments { get; set; }
    }
}

