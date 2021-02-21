using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;

namespace SignalR.EventAggregatorProxy.Demo.CqsClient
{
    public interface ICqsClient
    {
        Task ExecuteCommand<TCommand>(TCommand cmd) where TCommand : ICommand;
    }
}