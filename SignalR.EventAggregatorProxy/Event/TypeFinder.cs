using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.Event
{
    public class TypeFinder : ITypeFinder
    {
        private readonly IAssemblyLocator assemblyLocator;
        private readonly IDictionary<string, Type> eventTypeLookup;
        private readonly IDictionary<string, Type> types;
        private readonly IDictionary<Type, IEnumerable<Type>> constraintHandlerTypes;
        
        public TypeFinder(IAssemblyLocator assemblyLocator, IEventTypeFinder eventTypeFinder)
        {
            this.assemblyLocator = assemblyLocator;
            types = new Dictionary<string, Type>();

            var eventTypes = eventTypeFinder
                    .ListEventsTypes();

            eventTypeLookup = eventTypes
                .ToDictionary(t => t.GetFullNameWihoutGenerics(), t => t);

            var lookupType = typeof(IEventConstraintHandler<>);
            var predicate = new Func<Type, bool>(t => t.IsGenericType && t.GetGenericTypeDefinition() == lookupType);

            var lookup = assemblyLocator
                .GetAssemblies()
                .SelectMany(GetTypesSafely)
                .Where(t => t.GetInterfaces().Any(predicate))
                .Select(t => new { Handler = t, Type = t.GetInterfaces().First(predicate).GetGenericArguments()[0] })
                .GroupBy(t => t.Type)
                .ToDictionary(g => g.Key, g => g.Select(t => t.Handler));

            constraintHandlerTypes = eventTypes
                .SelectMany(ListInheritanceChange)
                .Distinct()
                .ToDictionary(type => type, type => lookup
                    .Where(kvp => kvp.Key.IsAssignableFrom(type))
                    .SelectMany(kvp => kvp.Value)
                    .ToList() as IEnumerable<Type>);
        }

        private IEnumerable<Type> ListInheritanceChange(Type type)
        {
            if(type == null) yield break;

            yield return type;
            foreach (var t in ListInheritanceChange(type.BaseType))
                yield return t;
        }

        private IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return new List<Type>();
            }
        } 

        public IEnumerable<Type> ListEventTypes()
        {
            return eventTypeLookup.Values;
        }

        public Type GetEventType(string typeName)
        {
            return eventTypeLookup[typeName];
        }

        public Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                if (!types.ContainsKey(typeName))
                {
                    types[typeName] = assemblyLocator.GetAssemblies().Select(a => a.GetType(typeName)).Single(t => t != null);
                }
                type = types[typeName];
            }

            return type;
        }
        public IEnumerable<Type> GetConstraintHandlerTypes(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            return constraintHandlerTypes[type];
        }
    }
}
