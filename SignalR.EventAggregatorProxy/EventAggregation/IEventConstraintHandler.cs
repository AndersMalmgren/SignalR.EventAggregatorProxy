using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public interface IEventConstraintHandler
    {
        bool Allow(object message, HubCallerContext context, dynamic constraint);
    }

    public interface IEventConstraintHandler<T> : IEventConstraintHandler
    {
        
        bool Allow(T message, HubCallerContext context, dynamic constraint);
    }

    public abstract class EventConstraintHandler<T> : IEventConstraintHandler<T>
    {
        public bool Allow(object message, HubCallerContext context, dynamic constraint)
        {
            return Allow((T) message, context, constraint);
        }
        public abstract bool Allow(T message, HubCallerContext context, dynamic constraint);
    }
}
