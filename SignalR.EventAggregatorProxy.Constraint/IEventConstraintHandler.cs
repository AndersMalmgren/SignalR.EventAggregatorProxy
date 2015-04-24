namespace SignalR.EventAggregatorProxy.Constraint
{
    public interface IEventConstraintHandler
    {
        bool Allow(object message, ConstraintContext context, dynamic constraint);
    }

    public interface IEventConstraintHandler<T> : IEventConstraintHandler
    {

		bool Allow(T message, ConstraintContext context, dynamic constraint);
    }
}
