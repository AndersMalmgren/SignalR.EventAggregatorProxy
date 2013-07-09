using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.Events
{
    public class ConstrainedEvent : StandardEvent
    {
        public ConstrainedEvent(string message) : base(message)
        {
        }
    }
}