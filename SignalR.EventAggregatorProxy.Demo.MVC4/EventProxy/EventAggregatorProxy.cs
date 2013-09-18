using System;
using Caliburn.Micro;
using IEventAggregator = SignalR.EventAggregatorProxy.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.EventProxy
{
    public class EventAggregatorProxy : IEventAggregator, IHandle<Contracts.Events.Event>
    {
        private Action<object> handler;

        public EventAggregatorProxy(Caliburn.Micro.IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Subscribe(Action<object> handler)
        {
            this.handler = handler;
        }

        public void Handle(Contracts.Events.Event message)
        {
            if(handler != null) //Events can come in before the subsriber is hooked up
                handler(message);
        }
    }
}