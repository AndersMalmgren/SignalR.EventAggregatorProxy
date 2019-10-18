using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Model
{
    public class EventType
    {
        public string Type { get; set; }
        public string[] GenericArguments { get; set; }
        public int? ConstraintId { get; set; }
    }
}
