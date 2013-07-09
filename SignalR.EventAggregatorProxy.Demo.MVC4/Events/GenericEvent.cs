using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.Events
{
    public class GenericEvent<T> : Event
    {
        public T Message { get; set; }

        public GenericEvent(T message)
        {
            Message = message;
        } 
    }
}