using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Demo.MVC4.Events;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.EventConstraintHandlers
{
    public class ConstrainedEventConstraintHandler : EventConstraintHandler<ConstrainedEvent, ConstrainedEventConstraint>
    {
        public override bool Allow(ConstrainedEvent message, string username, ConstrainedEventConstraint constraint)
        {
            return message.Message == constraint.Message;
        }
    }

    public class ConstrainedEventConstraint
    {
        public string Message { get; set; }
    }
}