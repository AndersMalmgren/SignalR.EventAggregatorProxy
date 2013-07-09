using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Ninject;
using Ninject.Syntax;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.App_Start.DependencyResolvers
{
    public class NinjectDependencyScope : IDependencyScope
    {
        IResolutionRoot resolver;

        public NinjectDependencyScope(IResolutionRoot resolver)
        {
            this.resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            return resolver.TryGet(serviceType);
        }

        public System.Collections.Generic.IEnumerable<object> GetServices(Type serviceType)
        {
            return resolver.GetAll(serviceType);
        }

        public void Dispose()
        {
        }
    }

    // This class is the resolver, but it is also the global scope
    // so we derive from NinjectScope.
    public class WebApiDependencyResolver : NinjectDependencyScope, IDependencyResolver
    {
        IKernel kernel;

        public WebApiDependencyResolver(IKernel kernel)
            : base(kernel)
        {
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(kernel);
        }
    }
}