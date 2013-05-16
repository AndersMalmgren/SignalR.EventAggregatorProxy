using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public class Subscription
    {
        public Subscription(string connectionId, Type eventType, dynamic constraint)
        {
            ConnectionId = connectionId;
            EventType = eventType;
            Constraint = constraint;
        }

        public string ConnectionId { get; set; }
        public Type EventType { get; set; }
        public dynamic Constraint { get; set; }
    }
}

