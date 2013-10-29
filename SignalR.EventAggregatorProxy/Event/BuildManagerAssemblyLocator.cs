using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;

namespace SignalR.EventAggregatorProxy.Event
{
    public class BuildManagerAssemblyLocator : IAssemblyLocator
    {
        public IEnumerable<Assembly> GetAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().Cast<Assembly>();
        }
    }
}
