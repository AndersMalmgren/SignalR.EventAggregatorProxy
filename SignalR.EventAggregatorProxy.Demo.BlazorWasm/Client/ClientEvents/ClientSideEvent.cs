using SignalR.EventAggregatorProxy.Demo.Contracts.Events;

namespace SignalR.EventAggregatorProxy.Demo.BlazorWasm.Client.ClientEvents
{
    public class ClientSideEvent : IMessageEvent<string>
    {
        public ClientSideEvent(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
