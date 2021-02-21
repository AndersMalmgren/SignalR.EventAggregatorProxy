using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.Contracts.Commands
{
    public class EventCommand<TEvent> : ICommand where TEvent : IMessageEvent<string>
    {
        public string Message { get; set; }
    }
}
