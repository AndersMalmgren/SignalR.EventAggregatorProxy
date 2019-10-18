using System.Text.Json;

namespace SignalR.EventAggregatorProxy.Constraint
{
    public interface IEventConstraintHandler
    {
        bool Allow(object message, ConstraintContext context, JsonElement constraint);
    }

    public interface IEventConstraintHandler<T> : IEventConstraintHandler
    {

        bool Allow(T message, ConstraintContext context, JsonElement constraint);
    }
}
