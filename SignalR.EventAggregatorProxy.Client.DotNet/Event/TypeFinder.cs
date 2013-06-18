using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Client.Extensions;

namespace SignalR.EventAggregatorProxy.Client.Event
{
    public class TypeFinder<TEvent>
    {
        private Dictionary<string, Type> eventTypes;
        private Dictionary<string, Type> types; 

        public TypeFinder()
        {
            var type = typeof (TEvent);
            eventTypes = type.Assembly.GetTypes()
                       .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                       .ToDictionary(t => t.GetFullNameWihoutGenerics(), t => t);

            types = new Dictionary<string, Type>();
        }

        public Type GetEventType(string type)
        {
            return eventTypes[type];
        }

        public Type GetType(string type)
        {
            if (!types.ContainsKey(type))
            {
                types[type] = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(type))
                .First(t => t != null);
            }

            return types[type];
        }
    }
}
