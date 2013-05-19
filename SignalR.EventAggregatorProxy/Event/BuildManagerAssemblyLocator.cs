using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using Microsoft.AspNet.SignalR.SystemWeb.Infrastructure;

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
