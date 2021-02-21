using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Caliburn.Micro;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Demo.CqsClient;
using SignalR.EventAggregatorProxy.Demo.DotNetCore.ClientEvents;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.DotNetCore.Views
{
    public class SendMessageViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ICqsClient client;
        private string message;

        public SendMessageViewModel(IEventAggregator eventAggregator, ICqsClient client)
        {
            this.eventAggregator = eventAggregator;
            this.client = client;
        }

        public string Message
        {
            get { return message; }
            set { 
                message = value;

                CanFireConstrainedEvent = 
                    CanFireGenericEvent = 
                    CanFireStandardEvent = 
                    CanFireClientSideEvent =
                    !string.IsNullOrEmpty(value);

                NotifyOfPropertyChange(() => CanFireConstrainedEvent);
                NotifyOfPropertyChange(() => CanFireGenericEvent);
                NotifyOfPropertyChange(() => CanFireStandardEvent);
                NotifyOfPropertyChange(() => CanFireClientSideEvent);
            }
        }

        public bool CanFireStandardEvent { get; private set; }
        public Task FireStandardEvent()
        {
            return Post<StandardEvent>();
        }

        public bool CanFireGenericEvent { get; private set; }
        public Task FireGenericEvent()
        {
            return Post<GenericEvent<string>>();
        }

        public bool CanFireConstrainedEvent { get; private set; }
        public Task FireConstrainedEvent()
        {
            return Post<ConstrainedEvent>();
        }

        public bool CanFireClientSideEvent { get; private set; }
        public void FireClientSideEvent()
        {
            eventAggregator.Publish(new ClientSideEvent(Message));
        }

        private async Task Post<TEvent>() where TEvent : IMessageEvent<string>
        {
            var cmd = new EventCommand<TEvent> { Message = message };
            await client.ExecuteCommand(cmd);
        }
    }
}
