using Caliburn.Micro;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Demo.DotNet.ClientEvents;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation;
using SignalR.EventAggregatorProxy.Demo.Contracts.Constraints;

namespace SignalR.EventAggregatorProxy.Demo.DotNet.Views
{
    public class MainShellViewModel : Screen, Client.DotNetCore.EventAggregation.IHandle<StandardEvent>, Client.DotNetCore.EventAggregation.IHandle<GenericEvent<string>>, Client.DotNetCore.EventAggregation.IHandle<ConstrainedEvent>, Client.DotNetCore.EventAggregation.IHandle<ClientSideEvent>
    {
        public MainShellViewModel(IProxyEventAggregator eventAggregator, SendMessageViewModel sendMessage)
        {
            SendMessage = sendMessage;
            DisplayName = ".NET Client Demo";


            Events = new BindableCollection<IMessageEvent<string>>();

            eventAggregator.Subscribe(this, builder => builder.For<ConstrainedEvent>().Add(new ConstrainedEventConstraint { Message = "HelloWorld" }));
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
