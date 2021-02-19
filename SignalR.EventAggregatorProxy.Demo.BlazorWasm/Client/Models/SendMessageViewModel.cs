using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.ClientEvents;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.Models
{
    public class SendMessageViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly HttpClient http;
        private string message;

        public SendMessageViewModel(IEventAggregator eventAggregator, HttpClient http)
        {
            this.eventAggregator = eventAggregator;
            this.http = http;
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
            return Post("fireStandardEvent");
        }

        public Task FireGenericEvent()
        {
            return Post("fireGenericEvent");
        }

        public Task FireConstrainedEvent()
        {
            return Post("fireConstrainedEvent");
        }

        public void FireClientSideEvent()
        {
            eventAggregator.Publish(new ClientSideEvent(Message));
        }

        private async Task Post(string method)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"/api/service/{method}"));
            request.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Message))));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await http
                .SendAsync(request);

            if(!result.IsSuccessStatusCode) throw new Exception(await result.Content.ReadAsStringAsync());
        }
    }
}
