using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Caliburn.Micro;
using SignalR.EventAggregatorProxy.Demo.DotNet.ClientEvents;
using IEventAggregator = SignalR.EventAggregatorProxy.Client.EventAggregation.IEventAggregator;

namespace SignalR.EventAggregatorProxy.Demo.DotNet.Views
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
        public void FireStandardEvent()
        {
            Post("fireStandardEvent");
        }

        public bool CanFireGenericEvent { get; private set; }
        public void FireGenericEvent()
        {
            Post("fireGenericEvent");
        }

        public bool CanFireConstrainedEvent { get; private set; }
        public void FireConstrainedEvent()
        {
            Post("fireConstrainedEvent");
        }

        public bool CanFireClientSideEvent { get; private set; }
        public void FireClientSideEvent()
        {
            eventAggregator.Publish(new ClientSideEvent(Message));
        }

        private void Post(string method)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:2336/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.PostAsJsonAsync(string.Format("api/service/{0}", method), Message);

        }
    }
}
