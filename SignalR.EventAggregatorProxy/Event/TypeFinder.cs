using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Event
{
    public class TypeFinder<TEvent> : ITypeFinder
    {
        private readonly IAssemblyLocator assemblyLocator;
        private IDictionary<string, Type> types;
        private IDictionary<Type, Type> constraintHandlerTypes;
        
        public TypeFinder()
        {
            assemblyLocator = GlobalHost.DependencyResolver.Resolve<IAssemblyLocator>();

            InitEventTypes();
            InitConstraintHandlerTypes();
        }

        private void InitEventTypes()
        {
            var type = typeof (TEvent);
            types = assemblyLocator.GetAssemblies()
                                   .SelectMany(a => a.GetTypes())
                                   .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                                   .ToDictionary(t => t.FullName, t => t);
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
            return types.Values;
        }

        public Type GetType(string type)
        {
            return types[type];
        }

        public Type GetConstraintHandlerType(Type type)
        {
            return constraintHandlerTypes.ContainsKey(type) ? constraintHandlerTypes[type] : null;
        }
    }
}
