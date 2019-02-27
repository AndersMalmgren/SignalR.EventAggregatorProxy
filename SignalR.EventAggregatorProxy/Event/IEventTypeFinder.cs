using System;
using System.Collections.Generic;
using System.Text;

namespace SignalR.EventAggregatorProxy.Event
{
    public interface IEventTypeFinder
    {
        IEnumerable<Type> ListEventsTypes();
    }
}
