namespace SignalR.EventAggregatorProxy.Constraint
{
    public abstract class EventConstraintHandler<T> : IEventConstraintHandler<T>
    {
        public bool Allow(object message, string username, dynamic constraint)
        {
            return Allow((T)message, username, constraint);
        }
        public abstract bool Allow(T message, string username, dynamic constraint);
    }
}