using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Tests.DotNetClient
{
    public abstract class Event
    {

    }

    public class StandardEvent : Event
    {

    }

    public class GenericEvent<T> : Event
    {
        public T FooBar { get; set; }
    }

    public class StandardEventConstraint
    {
        public int Id { get; set; }
    }
   
}
