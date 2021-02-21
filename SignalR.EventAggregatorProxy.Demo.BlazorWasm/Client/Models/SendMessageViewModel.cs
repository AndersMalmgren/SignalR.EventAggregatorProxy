using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.ClientEvents;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using SignalR.EventAggregatorProxy.Demo.CqsClient;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.Models
{
    public class SendMessageViewModel
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
                CanFireEvent = !string.IsNullOrEmpty(value);
            }
        }

        public bool CanFireEvent { get; private set; }

        public Task FireStandardEvent()
        {
            return Post<StandardEvent>();
        }

        public Task FireGenericEvent()
        {
            return Post<GenericEvent<string>>();
        }

        public Task FireConstrainedEvent()
        {
            return Post<ConstrainedEvent>();
        }

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
