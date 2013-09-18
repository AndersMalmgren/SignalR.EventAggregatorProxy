using System.Web.Http;
using Caliburn.Micro;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.MVC4.Controllers
{
    public class ServiceController : ApiController
    {
        private readonly IEventAggregator eventAggregator;

        public ServiceController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void Get()
        {
        }

        [HttpPost]
        public void FireStandardEvent([FromBody]string text)
        {
            eventAggregator.Publish(new StandardEvent(text));
        }

        [HttpPost]
        public void FireGenericEvent([FromBody]string text)
        {
            eventAggregator.Publish(new GenericEvent<string>(text));
        }

        [HttpPost]
        public void FireConstrainedEvent([FromBody] string text)
        {
            eventAggregator.Publish(new ConstrainedEvent(text));
        }
    }
}