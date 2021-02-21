using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore.CommandHandlers
{
    public class EventCommandHandler<TEvent> : ICommandHandler<EventCommand<TEvent>> where TEvent : IMessageEvent<string>, new()
    {
        private readonly IEventAggregator eventAggregator;

        public EventCommandHandler(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public async Task Handle(EventCommand<TEvent> cmd)
        {
            var e = new TEvent() {Message = cmd.Message};
            await eventAggregator.Publish(e);
        }
    }
    
}