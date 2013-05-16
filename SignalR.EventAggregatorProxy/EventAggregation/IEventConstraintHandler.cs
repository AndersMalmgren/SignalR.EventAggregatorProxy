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
        bool Allow(object message, string username, dynamic constraint);
    }

    public interface IEventConstraintHandler<T> : IEventConstraintHandler
    {

        bool Allow(T message, string username, dynamic constraint);
    }

    public abstract class EventConstraintHandler<T> : IEventConstraintHandler<T>
    {
        public bool Allow(object message, string username, dynamic constraint)
        {
            return Allow((T)message, username, constraint);
        }
        public abstract bool Allow(T message, string username, dynamic constraint);
    }
}
