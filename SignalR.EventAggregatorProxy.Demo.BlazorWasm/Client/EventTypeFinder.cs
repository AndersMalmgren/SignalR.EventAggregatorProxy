using System;
using System.Collections.Generic;
using System.Linq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Event;

namespace SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client
{

    public class EventTypeFinder : IEventTypeFinder
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
