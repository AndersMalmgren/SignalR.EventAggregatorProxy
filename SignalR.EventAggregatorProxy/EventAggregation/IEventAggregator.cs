using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public interface IEventAggregator
    {
        void Subscribe(Action<object> handler);
    }
}
