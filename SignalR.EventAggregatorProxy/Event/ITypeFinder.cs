using System;
using System.Collections.Generic;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Event
{
    public interface ITypeFinder
    {
        IEnumerable<Type> ListEventTypes();
        Type GetEventType(string type);
        IEnumerable<Type> GetConstraintHandlerTypes(Type type);
        Type GetType(string typeName);
    }
}