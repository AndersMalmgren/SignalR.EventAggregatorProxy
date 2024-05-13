using System;
using System.Collections.Generic;
using System.Linq;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Client.DotNetCore.Extensions;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Event
{
    public interface ITypeFinder
    {
        Type GetEventType(string type);
        Type GetType(string type);
        IEnumerable<Type> GetSubscriberEventTypes(object subscriber);
    }

    public class TypeFinder : ITypeFinder
    {
        private readonly Dictionary<string, Type> eventTypeLookup;
        private readonly Dictionary<string, Type> types;
        private readonly IEnumerable<Type> eventTypes;
        private readonly Dictionary<Type, IEnumerable<Type>> subscriberLookup;


        public TypeFinder(IEventTypeFinder eventTypeFinder)
        {
            eventTypes = eventTypeFinder.ListEventsTypes().ToList();
            eventTypeLookup = eventTypes
                .ToDictionary(t => t.GetFullNameWihoutGenerics(), t => t);

            types = new Dictionary<string, Type>();
            subscriberLookup = new Dictionary<Type, IEnumerable<Type>>();
        }



        public Type GetEventType(string type)
        {
            return eventTypeLookup[type];
        }

        public Type GetType(string type)
        {
            if (!types.ContainsKey(type))
            {
                types[type] = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(type))
                .First(t => t != null)
                .NotNull();
            }

            return types[type];
        }

        public IEnumerable<Type> GetSubscriberEventTypes(object subscriber)
        {
            var subscriberType = subscriber.GetType();
            if (!subscriberLookup.ContainsKey(subscriberType))
            {
                var handleType = typeof(IHandle<>);
                subscriberLookup[subscriberType] = subscriberType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handleType && eventTypes.Any(t => t.IsAssignableFrom(i.GetGenericArguments()[0])))
                    .Select(i => i.GetGenericArguments()[0]);
            }

            return subscriberLookup[subscriberType];
        }
    }
}
