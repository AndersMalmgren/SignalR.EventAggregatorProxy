using System;
using System.Collections.Generic;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Event
{
    public interface ITypeFinder
    {
        IEnumerable<Type> ListEventTypes();
        Type GetType(string type);
        Type GetConstraintHandlerType(Type type);
    }
}