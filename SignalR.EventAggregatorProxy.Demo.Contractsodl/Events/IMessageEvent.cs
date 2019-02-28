using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Demo.Contracts.Events
{
    public interface IMessageEvent<TMessage>
    {
        TMessage Message { get; set; }
    }
}
