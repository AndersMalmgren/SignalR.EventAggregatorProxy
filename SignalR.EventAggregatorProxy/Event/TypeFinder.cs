using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.SystemWeb.Infrastructure;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Event
{
    public class TypeFinder<TEvent> : ITypeFinder
    {
        private IDictionary<string, Type> types;
        private IDictionary<Type, Type> constraintHandlerTypes;

        public TypeFinder()
        {
            InitEventTypes();
            InitConstraintHandlerTypes();
        }

        private void InitEventTypes()
        {
            var type = typeof (TEvent);
            var assemblyLocator = new BuildManagerAssemblyLocator();
            types = assemblyLocator.GetAssemblies()
                                   .SelectMany(a => a.GetTypes())
                                   .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                                   .ToDictionary(t => t.FullName, t => t);
        }

        private void InitConstraintHandlerTypes()
        {
            var lookupType = typeof(IEventConstraintHandler<>);
            var assemblyLocator = new BuildManagerAssemblyLocator();
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
