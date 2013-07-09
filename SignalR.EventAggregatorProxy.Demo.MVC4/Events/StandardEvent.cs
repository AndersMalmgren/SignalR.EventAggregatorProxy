using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.Events
{
    public class StandardEvent : Event
    {
        public string Message { get; set; }

        public StandardEvent(string message)
        {
            Message = message;
        }
    }
}