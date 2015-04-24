using Newtonsoft.Json.Linq;

namespace SignalR.EventAggregatorProxy.Constraint
{
    public abstract class EventConstraintHandler<TEvent> : IEventConstraintHandler<TEvent>
    {
		public bool Allow(object message, ConstraintContext context, dynamic constraint)
        {
            return Allow((TEvent)message, context, constraint);
        }
		public abstract bool Allow(TEvent message, ConstraintContext context, dynamic constraint);
    }

    public abstract class EventConstraintHandler<TEvent, TConstraint> : EventConstraintHandler<TEvent> where TConstraint : class
    {
		public override bool Allow(TEvent message, ConstraintContext context, dynamic constraint)
        {
            var jObject = constraint as JObject;
            var staticTypedConstraint = jObject != null ? jObject.ToObject<TConstraint>() : null;

            return Allow(message, context, staticTypedConstraint);
        }
		public abstract bool Allow(TEvent message, ConstraintContext context, TConstraint constraint);
    }
}