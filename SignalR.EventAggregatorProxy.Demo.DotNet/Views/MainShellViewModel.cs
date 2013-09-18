using Caliburn.Micro;
using SignalR.EventAggregatorProxy.Client.Constraint;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.Contracts.Constraints;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Demo.DotNet.ClientEvents;

namespace SignalR.EventAggregatorProxy.Demo.DotNet.Views
{
    public class MainShellViewModel : Screen, Client.EventAggregation.IHandle<StandardEvent>, Client.EventAggregation.IHandle<GenericEvent<string>>, Client.EventAggregation.IHandle<ConstrainedEvent>, Client.EventAggregation.IHandle<ClientSideEvent>
    {
        public MainShellViewModel(IEventAggregator<Event> eventAggregator, SendMessageViewModel sendMessage)
        {
            SendMessage = sendMessage;
            DisplayName = ".NET Client Demo";


            Events = new BindableCollection<IMessageEvent<string>>();

            eventAggregator.Subscribe(this, new[]
                {
                    new ConstraintInfo<ConstrainedEvent, ConstrainedEventConstraint>(new ConstrainedEventConstraint { Message = "HelloWorld" })
                });
        }

        public SendMessageViewModel SendMessage { get; private set; }

        public BindableCollection<IMessageEvent<string>> Events { get; private set; }

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
