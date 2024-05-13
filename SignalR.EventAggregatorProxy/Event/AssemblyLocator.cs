using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.Event
{
    public class AssemblyLocator : IAssemblyLocator
    {
        public IEnumerable<Assembly> GetAssemblies()
        {
            return Assembly.GetEntryAssembly().NotNull().GetReferencedAssemblies().Select(Assembly.Load).Union([Assembly.GetEntryAssembly()]);
        }
    }
}
