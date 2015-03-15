using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.EventAggregation
{
    public interface IEventAggregatorProxy<in TEvent>
    {
        void Publish(TEvent message);
    }
}
