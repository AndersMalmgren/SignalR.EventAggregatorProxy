using Newtonsoft.Json.Linq;

namespace SignalR.EventAggregatorProxy.Constraint
{
    public abstract class EventConstraintHandler<TEvent> : IEventConstraintHandler<TEvent>
    {
        public bool Allow(object message, string username, dynamic constraint)
        {
            return Allow((TEvent)message, username, constraint);
        }
        public abstract bool Allow(TEvent message, string username, dynamic constraint);
    }

    public abstract class EventConstraintHandler<TEvent, TConstraint> : EventConstraintHandler<TEvent> where TConstraint : class
    {
        public override bool Allow(TEvent message, string username, dynamic constraint)
        {
            var jObject = constraint as JObject;
            var staticTypedConstraint = jObject != null ? jObject.ToObject<TConstraint>() : null;

            return Allow(message, username, staticTypedConstraint);
        }
        public abstract bool Allow(TEvent message, string username, TConstraint constraint);
    }
}