using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SignalR.EventAggregatorProxy.Event
{
    public class AssemblyLocator : IAssemblyLocator
    {
        public IEnumerable<Assembly> GetAssemblies()
        {
            return Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select(Assembly.Load);
        }
    }
}
