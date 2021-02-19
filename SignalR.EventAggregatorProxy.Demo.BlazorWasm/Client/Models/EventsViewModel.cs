using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.ClientEvents;
using SignalR.EventAggregatorProxy.Demo.Contracts.Constraints;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.Models
{
        public class EventsViewModel : IHandle<StandardEvent>, IHandle<GenericEvent<string>>, IHandle<ConstrainedEvent>, IHandle<ClientSideEvent>
        {
            public EventsViewModel(IProxyEventAggregator eventAggregator)
            {
                Events = new List<IMessageEvent<string>>();
                eventAggregator.Subscribe(this, builder => builder.For<ConstrainedEvent>().Add(new ConstrainedEventConstraint { Message = "HelloWorld" }));
            }


            public List<IMessageEvent<string>> Events { get; }

            public void Handle(StandardEvent message)
            {
                Add(message);
            }

            public void Handle(GenericEvent<string> message)
            {
                Add(message);
            }

            public void Handle(ConstrainedEvent message)
            {
                Add(message);
            }

            public void Handle(ClientSideEvent message)
            {
                Add(message);
            }

            private void Add(IMessageEvent<string> message)
            {
                Events.Add(message);
            }
        }
}
