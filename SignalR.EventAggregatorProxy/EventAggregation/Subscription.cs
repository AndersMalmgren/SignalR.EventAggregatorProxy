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
        public Subscription(string connectionId, string username, dynamic constraint)
        {
            ConnectionId = connectionId;
            Username = username;
            Constraint = constraint;
        }

        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public dynamic Constraint { get; set; }
    }
}

