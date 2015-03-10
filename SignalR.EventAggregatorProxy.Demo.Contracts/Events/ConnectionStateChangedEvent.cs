using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Demo.Contracts.Events
{
    public class ConnectionStateChangedEvent : Event
    {
        public ConnectionStateChangedEvent(string connectionId, bool connected)
        {
            ConnectionId = connectionId;
            Connected = connected;
        }

        public string ConnectionId { get; private set; }
        public bool Connected { get; private set; }
    }
}
