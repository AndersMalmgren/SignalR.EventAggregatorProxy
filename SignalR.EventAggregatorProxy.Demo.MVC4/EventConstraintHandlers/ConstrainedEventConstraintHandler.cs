using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Demo.Contracts.Constraints;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.EventConstraintHandlers
{
    public class ConstrainedEventConstraintHandler : EventConstraintHandler<ConstrainedEvent, ConstrainedEventConstraint>
    {
        public override bool Allow(ConstrainedEvent message, string username, ConstrainedEventConstraint constraint)
        {
            return message.Message == constraint.Message;
        }
    }
}