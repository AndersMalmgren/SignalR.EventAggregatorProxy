using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Client.Bootstrap
{
   
    public class DependencyResolver
    {
        private Dictionary<Type, IResolver> resolvers;
        static DependencyResolver()
        {
            Global = Bootstrapper.Create();
        }

        public DependencyResolver()
        {
            resolvers = new Dictionary<Type, IResolver>();
        }

        public static DependencyResolver Global { get; private set; }

        public void Register<T>(Func<T> factory)
        {
            resolvers[typeof (T)] = new Resolver<T>(factory);
        }

        public T Get<T>()
        {
            var type = typeof (T);
            if (!resolvers.ContainsKey(type))
            {
                throw new ArgumentException(string.Format("{0} is not a registered type", type.FullName));
            }

            return (T)resolvers[type].Resolve();
        }
    }
}
