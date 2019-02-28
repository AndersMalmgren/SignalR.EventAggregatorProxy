using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IEventAggregator eventAggregator;

        public ServiceController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        [HttpPost]
        [Route("FireStandardEvent")]
        public Task FireStandardEvent([FromBody]string text)
        {
            return eventAggregator.Publish(new StandardEvent(text));
        }

        [HttpPost]
        [Route("FireGenericEvent")]
        public Task FireGenericEvent([FromBody]string text)
        {
            return eventAggregator.Publish(new GenericEvent<string>(text));
        }

        [HttpPost]
        [Route("FireConstrainedEvent")]
        public Task FireConstrainedEvent([FromBody] string text)
        {
            return eventAggregator.Publish(new ConstrainedEvent(text));
        }
    }
}