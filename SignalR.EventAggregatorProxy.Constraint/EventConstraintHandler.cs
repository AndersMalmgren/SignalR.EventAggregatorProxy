

using System.Text.Json;

namespace SignalR.EventAggregatorProxy.Constraint
{
    public abstract class EventConstraintHandler<TEvent> : IEventConstraintHandler<TEvent>
    {
        public bool Allow(object message, ConstraintContext context, JsonElement constraint)
        {
            return Allow((TEvent)message, context, constraint);
        }
        public abstract bool Allow(TEvent message, ConstraintContext context, JsonElement constraint);
    }

    public abstract class EventConstraintHandler<TEvent, TConstraint> : EventConstraintHandler<TEvent> where TConstraint : class
    {
        public override bool Allow(TEvent message, ConstraintContext context, JsonElement constraint)
        {
            var json = constraint.GetRawText();
            return Allow(message, context, JsonSerializer.Deserialize<TConstraint>(json, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true}));
        }
        public abstract bool Allow(TEvent message, ConstraintContext context, TConstraint constraint);
    }
}