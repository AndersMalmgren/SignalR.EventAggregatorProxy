namespace SignalR.EventAggregatorProxy.Constraint
{
    public interface IEventConstraintHandler
    {
        bool Allow(object message, string username, dynamic constraint);
    }

    public interface IEventConstraintHandler<T> : IEventConstraintHandler
    {

        bool Allow(T message, string username, dynamic constraint);
    }
}
