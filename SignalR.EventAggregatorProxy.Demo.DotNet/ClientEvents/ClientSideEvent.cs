using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.DotNet.ClientEvents
{
    public class ClientSideEvent : IMessageEvent<string>
    {
        public ClientSideEvent(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
