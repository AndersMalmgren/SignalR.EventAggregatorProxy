using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.Event
{
    public class TypeFinder<TEvent> : ITypeFinder
    {
        private readonly IAssemblyLocator assemblyLocator;
        private IDictionary<string, Type> eventTypes;
        private IDictionary<string, Type> types;
        private IDictionary<Type, Type> constraintHandlerTypes;
        
        public TypeFinder()
        {
            assemblyLocator = GlobalHost.DependencyResolver.Resolve<IAssemblyLocator>();
            types = new Dictionary<string, Type>();

            InitEventTypes();
            InitConstraintHandlerTypes();
        }

        private void InitEventTypes()
        {
            var type = typeof (TEvent);
            eventTypes = assemblyLocator.GetAssemblies()
                                   .SelectMany(a => a.GetTypes())
                                   .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                                   .ToDictionary(t => t.GetFullNameWihoutGenerics(), t => t);
        }

        private void InitConstraintHandlerTypes()
        {
            var lookupType = typeof(IEventConstraintHandler<>);
            constraintHandlerTypes = assemblyLocator
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Any(i => i.GUID == lookupType.GUID))
                .Select(t => new { Handler = t, Type = t.GetInterfaces().First(i => i.GUID == lookupType.GUID).GetGenericArguments()[0] })
                .ToDictionary(t => t.Type, t => t.Handler);
        }

        public IEnumerable<Type> ListEventTypes()
        {
            return eventTypes.Values;
        }

        public Type GetEventType(string typeName)
        {
            return eventTypes[typeName];
        }

        public Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                if (!types.ContainsKey(typeName))
                {
                    types[typeName] = assemblyLocator.GetAssemblies().Select(a => a.GetType(typeName)).Single();
                }
                type = types[typeName];
            }

            return type;
        }

        public Type GetConstraintHandlerType(Type type)
        {
            return constraintHandlerTypes.ContainsKey(type) ? constraintHandlerTypes[type] : null;
        }
    }
}
