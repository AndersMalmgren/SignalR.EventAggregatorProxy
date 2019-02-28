using System;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Demo.Contracts.Constraints;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore
{
    public class ConstrainedEventConstraintHandler : EventConstraintHandler<ConstrainedEvent, ConstrainedEventConstraint>
    {
        public override bool Allow(ConstrainedEvent message, ConstraintContext context, ConstrainedEventConstraint constraint)
        {
            return message.Message == constraint.Message;
        }
    }
}