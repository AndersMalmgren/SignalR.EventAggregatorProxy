using System;
using System.Collections.Generic;
using System.Linq;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore
{
    public partial class Startup
    {
        private class EventTypeFinder : IEventTypeFinder
        {
            private readonly List<Type> types;

            public EventTypeFinder()
            {
                var type = typeof(Contracts.Events.Event);
                types = type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t)).ToList();
            }


            public IEnumerable<Type> ListEventsTypes()
            {
                return types;
            }
        }
    }
}
