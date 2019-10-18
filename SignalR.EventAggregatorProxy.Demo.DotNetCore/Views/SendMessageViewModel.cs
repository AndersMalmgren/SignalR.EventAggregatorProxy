using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Caliburn.Micro;
using SignalR.EventAggregatorProxy.Demo.DotNetCore.ClientEvents;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.DotNetCore.Views
{
    public class SendMessageViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;
        private string message;

        public SendMessageViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
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
            return Post("fireStandardEvent");
        }

        public bool CanFireGenericEvent { get; private set; }
        public Task FireGenericEvent()
        {
            return Post("fireGenericEvent");
        }

        public bool CanFireConstrainedEvent { get; private set; }
        public Task FireConstrainedEvent()
        {
            return Post("fireConstrainedEvent");
        }

        public bool CanFireClientSideEvent { get; private set; }
        public void FireClientSideEvent()
        {
            eventAggregator.Publish(new ClientSideEvent(Message));
        }

        private async Task Post(string method)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"http://localhost:60976/api/service/{method}"));
            request.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Message))));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await client.SendAsync(request);
            if(!result.IsSuccessStatusCode) throw new Exception(await result.Content.ReadAsStringAsync());
        }
    }
}
