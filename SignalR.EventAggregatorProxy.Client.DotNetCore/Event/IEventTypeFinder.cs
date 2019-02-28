using System;
using System.Collections.Generic;
using System.Text;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Event
{
    public interface IEventTypeFinder
    {
        IEnumerable<Type> ListEventsTypes();
    }
}
